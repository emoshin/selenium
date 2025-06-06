// <copyright file="JavaScriptEngine.cs" company="Selenium Committers">
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

using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenQA.Selenium
{
    /// <summary>
    /// Provides methods allowing the user to manage settings in the browser's JavaScript engine.
    /// </summary>
    [RequiresUnreferencedCode("JavaScriptEngine is currently implemented with CDP. When it is implemented with BiDi, AOT will be supported")]
    [RequiresDynamicCode("JavaScriptEngine is currently implemented with CDP. When it is implemented with BiDi, AOT will be supported.")]
    public class JavaScriptEngine : IJavaScriptEngine
    {
        private const string MonitorBindingName = "__webdriver_attribute";

        private readonly IWebDriver driver;
        private readonly Lazy<DevToolsSession> session;
        private readonly Dictionary<string, InitializationScript> initializationScripts = new Dictionary<string, InitializationScript>();
        private readonly Dictionary<string, PinnedScript> pinnedScripts = new Dictionary<string, PinnedScript>();
        private readonly HashSet<string> bindings = new HashSet<string>();
        private bool isEnabled = false;
        private bool isDisposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptEngine"/> class.
        /// </summary>
        /// <param name="driver">The <see cref="IWebDriver"/> instance in which the JavaScript engine is executing.</param>
        public JavaScriptEngine(IWebDriver driver)
        {
            // Use of Lazy<T> means this exception won't be thrown until the user first
            // attempts to access the DevTools session, probably on the first call to
            // StartEventMonitoring() or in adding scripts to the instance.
            this.driver = driver;
            this.session = new Lazy<DevToolsSession>(() =>
            {
                if (driver is not IDevTools devToolsDriver)
                {
                    throw new WebDriverException("Driver must implement IDevTools to use these features");
                }

                return devToolsDriver.GetDevToolsSession();
            });
        }

        /// <summary>
        /// Occurs when a JavaScript callback with a named binding is executed.
        /// </summary>
        public event EventHandler<JavaScriptCallbackExecutedEventArgs>? JavaScriptCallbackExecuted;

        /// <summary>
        /// Occurs when an exception is thrown by JavaScript being executed in the browser.
        /// </summary>
        public event EventHandler<JavaScriptExceptionThrownEventArgs>? JavaScriptExceptionThrown;

        /// <summary>
        /// Occurs when methods on the JavaScript console are called.
        /// </summary>
        public event EventHandler<JavaScriptConsoleApiCalledEventArgs>? JavaScriptConsoleApiCalled;

        /// <summary>
        /// Occurs when a value of an attribute in an element is being changed.
        /// </summary>
        public event EventHandler<DomMutatedEventArgs>? DomMutated;

        /// <summary>
        /// Gets the read-only list of initialization scripts added for this JavaScript engine.
        /// </summary>
        public IReadOnlyList<InitializationScript> InitializationScripts
        {
            get
            {
                // Return a copy.
                return new List<InitializationScript>(this.initializationScripts.Values);
            }
        }

        /// <summary>
        /// Gets the read-only list of bindings added for this JavaScript engine.
        /// </summary>
        public IReadOnlyList<string> ScriptCallbackBindings
        {
            get
            {
                // Return a copy.
                return new List<string>(this.bindings);
            }
        }

        /// <summary>
        /// Asynchronously starts monitoring for events from the browser's JavaScript engine.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task StartEventMonitoring()
        {
            this.session.Value.Domains.JavaScript.BindingCalled += OnScriptBindingCalled;
            this.session.Value.Domains.JavaScript.ExceptionThrown += OnJavaScriptExceptionThrown;
            this.session.Value.Domains.JavaScript.ConsoleApiCalled += OnConsoleApiCalled;
            await this.EnableDomains().ConfigureAwait(false);
        }

        /// <summary>
        /// Stops monitoring for events from the browser's JavaScript engine.
        /// </summary>
        public void StopEventMonitoring()
        {
            this.session.Value.Domains.JavaScript.ConsoleApiCalled -= OnConsoleApiCalled;
            this.session.Value.Domains.JavaScript.ExceptionThrown -= OnJavaScriptExceptionThrown;
            this.session.Value.Domains.JavaScript.BindingCalled -= OnScriptBindingCalled;
        }

        /// <summary>
        /// Enables monitoring for DOM changes.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task EnableDomMutationMonitoring()
        {
            // Execute the script to have it enabled on the currently loaded page.
            string script = GetMutationListenerScript();
            await this.session.Value.Domains.JavaScript.Evaluate(script).ConfigureAwait(false);

            await this.AddScriptCallbackBinding(MonitorBindingName).ConfigureAwait(false);
            await this.AddInitializationScript(MonitorBindingName, script).ConfigureAwait(false);
        }

        /// <summary>
        /// Disables monitoring for DOM changes.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task DisableDomMutationMonitoring()
        {
            await this.RemoveScriptCallbackBinding(MonitorBindingName).ConfigureAwait(false);
            await this.RemoveInitializationScript(MonitorBindingName).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously adds JavaScript to be loaded on every document load.
        /// </summary>
        /// <param name="scriptName">The friendly name by which to refer to this initialization script.</param>
        /// <param name="script">The JavaScript to be loaded on every page.</param>
        /// <returns>A task containing an <see cref="InitializationScript"/> object representing the script to be loaded on each page.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="scriptName"/> or <paramref name="script"/> are <see langword="null"/>.</exception>
        public async Task<InitializationScript> AddInitializationScript(string scriptName, string script)
        {
            if (scriptName is null)
            {
                throw new ArgumentNullException(nameof(scriptName));
            }

            if (script is null)
            {
                throw new ArgumentNullException(nameof(script));
            }

            if (this.initializationScripts.TryGetValue(scriptName, out InitializationScript? existingScript))
            {
                return existingScript;
            }

            await this.EnableDomains().ConfigureAwait(false);

            string scriptId = await this.session.Value.Domains.JavaScript.AddScriptToEvaluateOnNewDocument(script).ConfigureAwait(false);

            InitializationScript initializationScript = new InitializationScript(scriptId, scriptName, script);
            this.initializationScripts[scriptName] = initializationScript;
            return initializationScript;
        }

        /// <summary>
        /// Asynchronously removes JavaScript from being loaded on every document load.
        /// </summary>
        /// <param name="scriptName">The friendly name of the initialization script to be removed.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="scriptName"/> is <see langword="null"/>.</exception>
        public async Task RemoveInitializationScript(string scriptName)
        {
            if (scriptName is null)
            {
                throw new ArgumentNullException(nameof(scriptName));
            }

            if (this.initializationScripts.TryGetValue(scriptName, out InitializationScript? script))
            {
                string scriptId = script.ScriptId;
                await this.session.Value.Domains.JavaScript.RemoveScriptToEvaluateOnNewDocument(scriptId).ConfigureAwait(false);
                this.initializationScripts.Remove(scriptName);
            }
        }

        /// <summary>
        /// Asynchronously removes all initialization scripts from being loaded on every document load.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task ClearInitializationScripts()
        {
            // Use a copy of the list to prevent the iterator from becoming invalid
            // when we modify the collection.
            List<string> scriptNames = new List<string>(this.initializationScripts.Keys);
            foreach (string scriptName in scriptNames)
            {
                await this.RemoveInitializationScript(scriptName).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Pins a JavaScript snippet for execution in the browser without transmitting the
        /// entire script across the wire for every execution.
        /// </summary>
        /// <param name="script">The JavaScript to pin</param>
        /// <returns>A task containing a <see cref="PinnedScript"/> object to use to execute the script.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="script"/> is <see langword="null"/>.</exception>
        public async Task<PinnedScript> PinScript(string script)
        {
            if (script == null)
            {
                throw new ArgumentNullException(nameof(script));
            }

            string newScriptHandle = Guid.NewGuid().ToString("N");

            // We do an "Evaluate" first so as to immediately create the script on the loaded
            // page, then will add it to the initialization of future pages.
            await this.EnableDomains().ConfigureAwait(false);

            string creationScript = PinnedScript.MakeCreationScript(newScriptHandle, script);
            await this.session.Value.Domains.JavaScript.Evaluate(creationScript).ConfigureAwait(false);
            string scriptId = await this.session.Value.Domains.JavaScript.AddScriptToEvaluateOnNewDocument(creationScript).ConfigureAwait(false);

            PinnedScript pinnedScript = new PinnedScript(script, newScriptHandle, scriptId);
            this.pinnedScripts[pinnedScript.Handle] = pinnedScript;
            return pinnedScript;
        }

        /// <summary>
        /// Unpins a previously pinned script from the browser.
        /// </summary>
        /// <param name="script">The <see cref="PinnedScript"/> object to unpin.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="script"/> is <see langword="null"/>.</exception>
        public async Task UnpinScript(PinnedScript script)
        {
            if (script == null)
            {
                throw new ArgumentNullException(nameof(script));
            }

            if (this.pinnedScripts.ContainsKey(script.Handle))
            {
                await this.session.Value.Domains.JavaScript.Evaluate(script.MakeRemovalScript()).ConfigureAwait(false);
                await this.session.Value.Domains.JavaScript.RemoveScriptToEvaluateOnNewDocument(script.ScriptId).ConfigureAwait(false);
                this.pinnedScripts.Remove(script.Handle);
            }
        }

        /// <summary>
        /// Asynchronously adds a binding to a callback method that will raise an event when the named
        /// binding is called by JavaScript executing in the browser.
        /// </summary>
        /// <param name="bindingName">The name of the callback that will trigger events when called by JavaScript executing in the browser.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="bindingName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If A binding with the specified name already exists.</exception>
        public async Task AddScriptCallbackBinding(string bindingName)
        {
            if (bindingName is null)
            {
                throw new ArgumentNullException(nameof(bindingName));
            }

            if (!this.bindings.Add(bindingName))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "A binding named {0} has already been added", bindingName));
            }

            await this.EnableDomains().ConfigureAwait(false);
            await this.session.Value.Domains.JavaScript.AddBinding(bindingName).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously removes a binding to a JavaScript callback.
        /// </summary>
        /// <param name="bindingName">The name of the callback to be removed.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="bindingName"/> is <see langword="null"/>.</exception>
        public async Task RemoveScriptCallbackBinding(string bindingName)
        {
            if (bindingName is null)
            {
                throw new ArgumentNullException(nameof(bindingName));
            }

            await this.session.Value.Domains.JavaScript.RemoveBinding(bindingName).ConfigureAwait(false);
            _ = this.bindings.Remove(bindingName);
        }

        /// <summary>
        /// Asynchronously removes all bindings to JavaScript callbacks.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task ClearScriptCallbackBindings()
        {
            // Use a copy of the list to prevent the iterator from becoming invalid
            // when we modify the collection.
            List<string> bindingList = new List<string>(this.bindings);
            foreach (string binding in bindingList)
            {
                await this.RemoveScriptCallbackBinding(binding).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Asynchronously removes all bindings to JavaScript callbacks, all
        /// initialization scripts from being loaded for each document, and unpins
        /// all pinned scripts.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task ClearAll()
        {
            await this.ClearPinnedScripts().ConfigureAwait(false);
            await this.ClearInitializationScripts().ConfigureAwait(false);
            await this.ClearScriptCallbackBindings().ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously removes all bindings to JavaScript callbacks, all
        /// initialization scripts from being loaded for each document, all
        /// pinned scripts, and stops listening for events.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task Reset()
        {
            this.StopEventMonitoring();
            await ClearAll().ConfigureAwait(false);
        }

        /// <summary>
        /// Releases all resources associated with this <see cref="JavaScriptEngine"/>.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources associated with this <see cref="JavaScriptEngine"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if the Dispose method was explicitly called; otherwise, <see langword="false"/>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    if (this.session.IsValueCreated)
                    {
                        this.session.Value.Dispose();
                    }
                }

                this.isDisposed = true;
            }
        }

        private async Task ClearPinnedScripts()
        {
            // Use a copy of the list to prevent the iterator from becoming invalid
            // when we modify the collection.
            List<string> scriptHandles = new List<string>(this.pinnedScripts.Keys);
            foreach (string scriptHandle in scriptHandles)
            {
                await this.UnpinScript(this.pinnedScripts[scriptHandle]).ConfigureAwait(false);
            }
        }

        private async Task EnableDomains()
        {
            if (!this.isEnabled)
            {
                await this.session.Value.Domains.JavaScript.EnablePage().ConfigureAwait(false);
                await this.session.Value.Domains.JavaScript.EnableRuntime().ConfigureAwait(false);
                this.isEnabled = true;
            }
        }

        private static string GetMutationListenerScript()
        {
            string listenerScript = string.Empty;
            using (Stream resourceStream = ResourceUtilities.GetResourceStream("mutation-listener.js", "mutation-listener.js"))
            {
                using (StreamReader resourceReader = new StreamReader(resourceStream))
                {
                    listenerScript = resourceReader.ReadToEnd();
                }
            }

            return listenerScript;
        }

        private void OnScriptBindingCalled(object? sender, BindingCalledEventArgs e)
        {
            if (e.Name == MonitorBindingName)
            {
                DomMutationData valueChangeData = JsonSerializer.Deserialize<DomMutationData>(e.Payload) ?? throw new JsonException("DomMutationData returned null");
                var locator = By.CssSelector($"*[data-__webdriver_id='{valueChangeData.TargetId}']");
                valueChangeData.Element = driver.FindElements(locator).FirstOrDefault();

                this.DomMutated?.Invoke(this, new DomMutatedEventArgs(valueChangeData));
            }

            this.JavaScriptCallbackExecuted?.Invoke(this, new JavaScriptCallbackExecutedEventArgs
            (
                scriptPayload: e.Payload,
                bindingName: e.Name
            ));
        }

        private void OnJavaScriptExceptionThrown(object? sender, ExceptionThrownEventArgs e)
        {
            this.JavaScriptExceptionThrown?.Invoke(this, new JavaScriptExceptionThrownEventArgs(e.Message));
        }


        private void OnConsoleApiCalled(object? sender, ConsoleApiCalledEventArgs e)
        {
            if (this.JavaScriptConsoleApiCalled != null)
            {
                for (int i = 0; i < e.Arguments.Count; i++)
                {
                    this.JavaScriptConsoleApiCalled(this, new JavaScriptConsoleApiCalledEventArgs
                    (
                        messageContent: e.Arguments[i].Value,
                        messageTimeStamp: e.Timestamp,
                        messageType: e.Type
                    ));
                }
            }
        }
    }
}
