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

package org.openqa.selenium.grid.distributor.gridmodel.redis;

import org.openqa.selenium.grid.config.Config;
import org.openqa.selenium.grid.config.ConfigException;

import java.net.URI;
import java.net.URISyntaxException;
import java.util.Optional;

import static org.openqa.selenium.grid.distributor.config.DistributorOptions.DISTRIBUTOR_SECTION;

public class RedisGridModelOptions {
  private final Config config;

  public RedisGridModelOptions(Config config) {
    this.config = config;
  }

  public URI getRedisServerUri() {
    Optional<URI> host = config.get(DISTRIBUTOR_SECTION, "redis-server").map(str -> {
      try {
        return new URI(str);
      } catch (URISyntaxException e) {
        throw new ConfigException("Redis URI is not a valid URI: " + str);
      }
    });

    if (host.isPresent()) {
      return host.get();
    }

    Optional<Integer> port = config.getInt(DISTRIBUTOR_SECTION, "redis-port");
    Optional<String> hostname = config.get(DISTRIBUTOR_SECTION, "redis-host");

    if (!(port.isPresent() && hostname.isPresent())) {
      throw new ConfigException("Unable to determine host and port for the redis");
    }

    try {
      return new URI(
        "redis",
        null,
        hostname.get(),
        port.get(),
        null,
        null,
        null);
    } catch (URISyntaxException e) {
      throw new ConfigException(
        "Redis uri configured through host (%s) and port (%d) is not a valid URI",
        hostname.get(),
        port.get());
    }
  }
}