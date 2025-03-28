// <copyright file="NetworkModule.cs" company="Selenium Committers">
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
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenQA.Selenium.BiDi.Communication;

namespace OpenQA.Selenium.BiDi.Modules.Network;

public sealed class NetworkModule(Broker broker) : Module(broker)
{
    internal async Task<Intercept> AddInterceptAsync(IEnumerable<InterceptPhase> phases, AddInterceptOptions? options = null)
    {
        var @params = new AddInterceptCommandParameters(phases, options?.Contexts, options?.UrlPatterns);

        var result = await Broker.ExecuteCommandAsync<AddInterceptCommand, AddInterceptResult>(new AddInterceptCommand(@params), options).ConfigureAwait(false);

        return result.Intercept;
    }

    internal async Task RemoveInterceptAsync(Intercept intercept, RemoveInterceptOptions? options = null)
    {
        var @params = new RemoveInterceptCommandParameters(intercept);

        await Broker.ExecuteCommandAsync(new RemoveInterceptCommand(@params), options).ConfigureAwait(false);
    }

    public async Task<Intercept> InterceptRequestAsync(Func<BeforeRequestSentEventArgs, Task> handler, AddInterceptOptions? interceptOptions = null, SubscriptionOptions? options = null)
    {
        var intercept = await AddInterceptAsync([InterceptPhase.BeforeRequestSent], interceptOptions).ConfigureAwait(false);

        await intercept.OnBeforeRequestSentAsync(handler, options).ConfigureAwait(false);

        return intercept;
    }

    public async Task<Intercept> InterceptResponseAsync(Func<ResponseStartedEventArgs, Task> handler, AddInterceptOptions? interceptOptions = null, SubscriptionOptions? options = null)
    {
        var intercept = await AddInterceptAsync([InterceptPhase.ResponseStarted], interceptOptions).ConfigureAwait(false);

        await intercept.OnResponseStartedAsync(handler, options).ConfigureAwait(false);

        return intercept;
    }

    public async Task SetCacheBehaviorAsync(CacheBehavior behavior, SetCacheBehaviorOptions? options = null)
    {
        var @params = new SetCacheBehaviorCommandParameters(behavior, options?.Contexts);

        await Broker.ExecuteCommandAsync(new SetCacheBehaviorCommand(@params), options).ConfigureAwait(false);
    }

    public async Task<Intercept> InterceptAuthAsync(Func<AuthRequiredEventArgs, Task> handler, AddInterceptOptions? interceptOptions = null, SubscriptionOptions? options = null)
    {
        var intercept = await AddInterceptAsync([InterceptPhase.AuthRequired], interceptOptions).ConfigureAwait(false);

        await intercept.OnAuthRequiredAsync(handler, options).ConfigureAwait(false);

        return intercept;
    }

    internal async Task ContinueRequestAsync(Request request, ContinueRequestOptions? options = null)
    {
        var @params = new ContinueRequestCommandParameters(request, options?.Body, options?.Cookies, options?.Headers, options?.Method, options?.Url);

        await Broker.ExecuteCommandAsync(new ContinueRequestCommand(@params), options).ConfigureAwait(false);
    }

    internal async Task ContinueResponseAsync(Request request, ContinueResponseOptions? options = null)
    {
        var @params = new ContinueResponseCommandParameters(request, options?.Cookies, options?.Credentials, options?.Headers, options?.ReasonPhrase, options?.StatusCode);

        await Broker.ExecuteCommandAsync(new ContinueResponseCommand(@params), options).ConfigureAwait(false);
    }

    internal async Task FailRequestAsync(Request request, FailRequestOptions? options = null)
    {
        var @params = new FailRequestCommandParameters(request);

        await Broker.ExecuteCommandAsync(new FailRequestCommand(@params), options).ConfigureAwait(false);
    }

    internal async Task ProvideResponseAsync(Request request, ProvideResponseOptions? options = null)
    {
        var @params = new ProvideResponseCommandParameters(request, options?.Body, options?.Cookies, options?.Headers, options?.ReasonPhrase, options?.StatusCode);

        await Broker.ExecuteCommandAsync(new ProvideResponseCommand(@params), options).ConfigureAwait(false);
    }

    internal async Task ContinueWithAuthAsync(Request request, AuthCredentials credentials, ContinueWithAuthOptions? options = null)
    {
        await Broker.ExecuteCommandAsync(new ContinueWithAuthCommand(new ContinueWithAuthParameters.Credentials(request, credentials)), options).ConfigureAwait(false);
    }

    internal async Task ContinueWithAuthAsync(Request request, ContinueWithDefaultAuthOptions? options = null)
    {
        await Broker.ExecuteCommandAsync(new ContinueWithAuthCommand(new ContinueWithAuthParameters.Default(request)), options).ConfigureAwait(false);
    }

    internal async Task ContinueWithAuthAsync(Request request, ContinueWithCancelledAuthOptions? options = null)
    {
        await Broker.ExecuteCommandAsync(new ContinueWithAuthCommand(new ContinueWithAuthParameters.Cancel(request)), options).ConfigureAwait(false);
    }

    public async Task<Subscription> OnBeforeRequestSentAsync(Func<BeforeRequestSentEventArgs, Task> handler, SubscriptionOptions? options = null)
    {
        return await Broker.SubscribeAsync("network.beforeRequestSent", handler, options).ConfigureAwait(false);
    }

    public async Task<Subscription> OnBeforeRequestSentAsync(Action<BeforeRequestSentEventArgs> handler, SubscriptionOptions? options = null)
    {
        return await Broker.SubscribeAsync("network.beforeRequestSent", handler, options).ConfigureAwait(false);
    }

    public async Task<Subscription> OnResponseStartedAsync(Func<ResponseStartedEventArgs, Task> handler, SubscriptionOptions? options = null)
    {
        return await Broker.SubscribeAsync("network.responseStarted", handler, options).ConfigureAwait(false);
    }

    public async Task<Subscription> OnResponseStartedAsync(Action<ResponseStartedEventArgs> handler, SubscriptionOptions? options = null)
    {
        return await Broker.SubscribeAsync("network.responseStarted", handler, options).ConfigureAwait(false);
    }

    public async Task<Subscription> OnResponseCompletedAsync(Func<ResponseCompletedEventArgs, Task> handler, SubscriptionOptions? options = null)
    {
        return await Broker.SubscribeAsync("network.responseCompleted", handler, options).ConfigureAwait(false);
    }

    public async Task<Subscription> OnResponseCompletedAsync(Action<ResponseCompletedEventArgs> handler, SubscriptionOptions? options = null)
    {
        return await Broker.SubscribeAsync("network.responseCompleted", handler, options).ConfigureAwait(false);
    }

    public async Task<Subscription> OnFetchErrorAsync(Func<FetchErrorEventArgs, Task> handler, SubscriptionOptions? options = null)
    {
        return await Broker.SubscribeAsync("network.fetchError", handler, options).ConfigureAwait(false);
    }

    public async Task<Subscription> OnFetchErrorAsync(Action<FetchErrorEventArgs> handler, SubscriptionOptions? options = null)
    {
        return await Broker.SubscribeAsync("network.fetchError", handler, options).ConfigureAwait(false);
    }

    public async Task<Subscription> OnAuthRequiredAsync(Func<AuthRequiredEventArgs, Task> handler, SubscriptionOptions? options = null)
    {
        return await Broker.SubscribeAsync("network.authRequired", handler, options).ConfigureAwait(false);
    }

    public async Task<Subscription> OnAuthRequiredAsync(Action<AuthRequiredEventArgs> handler, SubscriptionOptions? options = null)
    {
        return await Broker.SubscribeAsync("network.authRequired", handler, options).ConfigureAwait(false);
    }
}
