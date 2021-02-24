// Licensed to the Software Freedom Conservancy (SFC) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The SFC licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

package org.openqa.selenium.grid.sessionqueue.remote;

import org.openqa.selenium.grid.config.Config;
import org.openqa.selenium.grid.data.RequestId;
import org.openqa.selenium.grid.log.LoggingOptions;
import org.openqa.selenium.grid.security.AddSecretFilter;
import org.openqa.selenium.grid.security.Secret;
import org.openqa.selenium.grid.security.SecretOptions;
import org.openqa.selenium.grid.server.NetworkOptions;
import org.openqa.selenium.grid.sessionqueue.NewSessionQueuer;
import org.openqa.selenium.grid.sessionqueue.config.NewSessionQueuerOptions;
import org.openqa.selenium.grid.web.Values;
import org.openqa.selenium.internal.Require;
import org.openqa.selenium.remote.http.Filter;
import org.openqa.selenium.remote.http.HttpClient;
import org.openqa.selenium.remote.http.HttpRequest;
import org.openqa.selenium.remote.http.HttpResponse;
import org.openqa.selenium.remote.tracing.HttpTracing;
import org.openqa.selenium.remote.tracing.Tracer;

import java.io.UncheckedIOException;
import java.net.MalformedURLException;
import java.net.URI;
import java.util.List;
import java.util.Optional;

import static java.net.HttpURLConnection.HTTP_OK;
import static org.openqa.selenium.grid.sessionqueue.NewSessionQueue.SESSIONREQUEST_ID_HEADER;
import static org.openqa.selenium.grid.sessionqueue.NewSessionQueue.SESSIONREQUEST_TIMESTAMP_HEADER;
import static org.openqa.selenium.remote.http.HttpMethod.DELETE;
import static org.openqa.selenium.remote.http.HttpMethod.GET;
import static org.openqa.selenium.remote.http.HttpMethod.POST;

public class RemoteNewSessionQueuer extends NewSessionQueuer {

  private static final String timestampHeader = SESSIONREQUEST_TIMESTAMP_HEADER;
  private static final String reqIdHeader = SESSIONREQUEST_ID_HEADER;
  private final HttpClient client;
  private final Filter addSecret;

  public RemoteNewSessionQueuer(Tracer tracer, HttpClient client, Secret registrationSecret) {
    super(tracer, registrationSecret);
    this.client = Require.nonNull("HTTP client", client);

    Require.nonNull("Registration secret", registrationSecret);
    this.addSecret = new AddSecretFilter(registrationSecret);
  }

  public static NewSessionQueuer create(Config config) {
    Tracer tracer = new LoggingOptions(config).getTracer();
    URI uri = new NewSessionQueuerOptions(config).getSessionQueuerUri();
    HttpClient.Factory clientFactory = new NetworkOptions(config).getHttpClientFactory(tracer);

    SecretOptions secretOptions = new SecretOptions(config);
    Secret registrationSecret = secretOptions.getRegistrationSecret();

    try {
      return new RemoteNewSessionQueuer(
        tracer,
        clientFactory.createClient(uri.toURL()),
        registrationSecret);
    } catch (MalformedURLException e) {
      throw new UncheckedIOException(e);
    }
  }

  @Override
  public HttpResponse addToQueue(HttpRequest request) {
    HttpRequest upstream = new HttpRequest(POST, "/se/grid/newsessionqueuer/session");
    HttpTracing.inject(tracer, tracer.getCurrentContext(), upstream);
    upstream.setContent(request.getContent());
    return client.execute(upstream);
  }

  @Override
  public boolean retryAddToQueue(HttpRequest request, RequestId reqId) {
    HttpRequest upstream =
      new HttpRequest(POST, "/se/grid/newsessionqueuer/session/retry/" + reqId.toString());
    HttpTracing.inject(tracer, tracer.getCurrentContext(), upstream);
    upstream.setContent(request.getContent());
    upstream.setHeader(timestampHeader, request.getHeader(timestampHeader));
    upstream.setHeader(reqIdHeader, reqId.toString());
    HttpResponse response = client.with(addSecret).execute(upstream);
    return Values.get(response, Boolean.class);
  }

  @Override
  public Optional<HttpRequest> remove(RequestId reqId) {
    HttpRequest upstream =
      new HttpRequest(GET, "/se/grid/newsessionqueuer/session/" + reqId.toString());
    HttpTracing.inject(tracer, tracer.getCurrentContext(), upstream);
    HttpResponse response = client.with(addSecret).execute(upstream);

    if(response.getStatus()==HTTP_OK) {
      HttpRequest httpRequest = new HttpRequest(POST, "/session");
      httpRequest.setContent(response.getContent());
      httpRequest.setHeader(timestampHeader, response.getHeader(timestampHeader));
      httpRequest.setHeader(reqIdHeader, response.getHeader(reqIdHeader));
      return Optional.ofNullable(httpRequest);
    }

    return Optional.empty();
  }

  @Override
  public int clearQueue() {
    HttpRequest upstream = new HttpRequest(DELETE, "/se/grid/newsessionqueuer/queue");
    HttpTracing.inject(tracer, tracer.getCurrentContext(), upstream);
    HttpResponse response = client.with(addSecret).execute(upstream);

    return Values.get(response, Integer.class);
  }

  @Override
  public List<Object> getQueueContents() {
    HttpRequest upstream = new HttpRequest(GET, "/se/grid/newsessionqueuer/queue");
    HttpTracing.inject(tracer, tracer.getCurrentContext(), upstream);
    HttpResponse response = client.execute(upstream);
    return Values.get(response, List.class);
  }

  @Override
  public boolean isReady() {
    try {
      return client.execute(new HttpRequest(GET, "/readyz")).isSuccessful();
    } catch (RuntimeException e) {
      return false;
    }
  }

}
