// <copyright file="ChromiumOptions.cs" company="Selenium Committers">
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
using System.Collections.ObjectModel;
using System.IO;

namespace OpenQA.Selenium.Chromium
{
    /// <summary>
    /// Abstract class to manage options specific to Chromium-based browsers.
    /// </summary>
    public abstract class ChromiumOptions : DriverOptions
    {
        private const string ArgumentsChromeOption = "args";
        private const string BinaryChromeOption = "binary";
        private const string ExtensionsChromeOption = "extensions";
        private const string LocalStateChromeOption = "localState";
        private const string PreferencesChromeOption = "prefs";
        private const string DetachChromeOption = "detach";
        private const string DebuggerAddressChromeOption = "debuggerAddress";
        private const string ExcludeSwitchesChromeOption = "excludeSwitches";
        private const string MinidumpPathChromeOption = "minidumpPath";
        private const string MobileEmulationChromeOption = "mobileEmulation";
        private const string PerformanceLoggingPreferencesChromeOption = "perfLoggingPrefs";
        private const string WindowTypesChromeOption = "windowTypes";
        private const string UseSpecCompliantProtocolOption = "w3c";
        private bool useSpecCompliantProtocol = true;
        private readonly List<string> arguments = new List<string>();
        private readonly List<string> extensionFiles = new List<string>();
        private readonly List<string> encodedExtensions = new List<string>();
        private readonly List<string> excludedSwitches = new List<string>();
        private readonly List<string> windowTypes = new List<string>();
        private readonly Dictionary<string, object> additionalChromeOptions = new Dictionary<string, object>();
        private Dictionary<string, object>? userProfilePreferences;
        private Dictionary<string, object>? localStatePreferences;

        private string? mobileEmulationDeviceName;
        private ChromiumMobileEmulationDeviceSettings? mobileEmulationDeviceSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChromiumOptions"/> class.
        /// </summary>
        public ChromiumOptions() : base()
        {
            this.AddKnownCapabilityName(this.CapabilityName, "current ChromiumOptions class instance");
            this.AddKnownCapabilityName(CapabilityType.LoggingPreferences, "SetLoggingPreference method");
            this.AddKnownCapabilityName(this.LoggingPreferencesChromeOption, "SetLoggingPreference method");
            this.AddKnownCapabilityName(ChromiumOptions.ArgumentsChromeOption, "AddArguments method");
            this.AddKnownCapabilityName(ChromiumOptions.BinaryChromeOption, "BinaryLocation property");
            this.AddKnownCapabilityName(ChromiumOptions.ExtensionsChromeOption, "AddExtensions method");
            this.AddKnownCapabilityName(ChromiumOptions.LocalStateChromeOption, "AddLocalStatePreference method");
            this.AddKnownCapabilityName(ChromiumOptions.PreferencesChromeOption, "AddUserProfilePreference method");
            this.AddKnownCapabilityName(ChromiumOptions.DetachChromeOption, "LeaveBrowserRunning property");
            this.AddKnownCapabilityName(ChromiumOptions.DebuggerAddressChromeOption, "DebuggerAddress property");
            this.AddKnownCapabilityName(ChromiumOptions.ExcludeSwitchesChromeOption, "AddExcludedArgument property");
            this.AddKnownCapabilityName(ChromiumOptions.MinidumpPathChromeOption, "MinidumpPath property");
            this.AddKnownCapabilityName(ChromiumOptions.MobileEmulationChromeOption, "EnableMobileEmulation method");
            this.AddKnownCapabilityName(ChromiumOptions.PerformanceLoggingPreferencesChromeOption, "PerformanceLoggingPreferences property");
            this.AddKnownCapabilityName(ChromiumOptions.WindowTypesChromeOption, "AddWindowTypes method");
            this.AddKnownCapabilityName(ChromiumOptions.UseSpecCompliantProtocolOption, "UseSpecCompliantProtocol property");
        }

        /// <summary>
        /// Gets the vendor prefix to apply to Chromium-specific capability names.
        /// </summary>
        protected abstract string VendorPrefix { get; }

        private string LoggingPreferencesChromeOption => this.VendorPrefix + ":loggingPrefs";

        /// <summary>
        /// Gets the name of the capability used to store Chromium options in
        /// an <see cref="ICapabilities"/> object.
        /// </summary>
        public abstract string CapabilityName { get; }

        /// <summary>
        /// Gets or sets the location of the Chromium browser's binary executable file.
        /// </summary>
        public override string? BinaryLocation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Chromium should be left running after the
        /// ChromeDriver instance is exited. Defaults to <see langword="false"/>.
        /// </summary>
        public bool LeaveBrowserRunning { get; set; }

        /// <summary>
        /// Gets the list of arguments appended to the Chromium command line as a string array.
        /// </summary>
        public ReadOnlyCollection<string> Arguments => this.arguments.AsReadOnly();

        /// <summary>
        /// Gets the list of extensions to be installed as an array of base64-encoded strings.
        /// </summary>
        public ReadOnlyCollection<string> Extensions
        {
            get
            {
                List<string> allExtensions = new List<string>(this.encodedExtensions);
                foreach (string extensionFile in this.extensionFiles)
                {
                    byte[] extensionByteArray = File.ReadAllBytes(extensionFile);
                    string encodedExtension = Convert.ToBase64String(extensionByteArray);
                    allExtensions.Add(encodedExtension);
                }

                return allExtensions.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets or sets the address of a Chromium debugger server to connect to.
        /// Should be of the form "{hostname|IP address}:port".
        /// </summary>
        public string? DebuggerAddress { get; set; }

        /// <summary>
        /// Gets or sets the directory in which to store minidump files.
        /// </summary>
        public string? MinidumpPath { get; set; }

        /// <summary>
        /// Gets or sets the performance logging preferences for the driver.
        /// </summary>
        public ChromiumPerformanceLoggingPreferences? PerformanceLoggingPreferences { get; set; }

        /// <summary>
        /// Gets or sets the options for automating Chromium applications on Android.
        /// </summary>
        public ChromiumAndroidOptions? AndroidOptions { get; set; }

        /// <summary>
        /// Adds a single argument to the list of arguments to be appended to the browser executable command line.
        /// </summary>
        /// <param name="argument">The argument to add.</param>
        /// <exception cref="ArgumentException">If <paramref name="argument"/> is <see langword="null"/> or <see cref="string.Empty"/>.</exception>
        public void AddArgument(string argument)
        {
            if (string.IsNullOrEmpty(argument))
            {
                throw new ArgumentException("argument must not be null or empty", nameof(argument));
            }

            this.AddArguments(argument);
        }

        /// <summary>
        /// Adds arguments to be appended to the browser executable command line.
        /// </summary>
        /// <param name="argumentsToAdd">An array of arguments to add.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="argumentsToAdd"/> is <see langword="null"/>.</exception>
        public void AddArguments(params string[] argumentsToAdd)
        {
            this.AddArguments((IEnumerable<string>)argumentsToAdd);
        }

        /// <summary>
        /// Adds arguments to be appended to the browser executable command line.
        /// </summary>
        /// <param name="argumentsToAdd">An <see cref="IEnumerable{T}"/> object of arguments to add.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="argumentsToAdd"/> is <see langword="null"/>.</exception>
        public void AddArguments(IEnumerable<string> argumentsToAdd)
        {
            if (argumentsToAdd == null)
            {
                throw new ArgumentNullException(nameof(argumentsToAdd), "argumentsToAdd must not be null");
            }

            this.arguments.AddRange(argumentsToAdd);
        }

        /// <summary>
        /// Adds a single argument to be excluded from the list of arguments passed by default
        /// to the browser executable command line by chromedriver.exe.
        /// </summary>
        /// <param name="argument">The argument to exclude.</param>
        /// <exception cref="ArgumentException">If <paramref name="argument"/> is <see langword="null"/> or <see cref="string.Empty"/>.</exception>
        public void AddExcludedArgument(string argument)
        {
            if (string.IsNullOrEmpty(argument))
            {
                throw new ArgumentException("argument must not be null or empty", nameof(argument));
            }

            this.AddExcludedArguments(argument);
        }

        /// <summary>
        /// Adds arguments to be excluded from the list of arguments passed by default
        /// to the browser executable command line by chromedriver.exe.
        /// </summary>
        /// <param name="argumentsToExclude">An array of arguments to exclude.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="argumentsToExclude"/> is <see langword="null"/>.</exception>
        public void AddExcludedArguments(params string[] argumentsToExclude)
        {
            this.AddExcludedArguments((IEnumerable<string>)argumentsToExclude);
        }

        /// <summary>
        /// Adds arguments to be excluded from the list of arguments passed by default
        /// to the browser executable command line by chromedriver.exe.
        /// </summary>
        /// <param name="argumentsToExclude">An <see cref="IEnumerable{T}"/> object of arguments to exclude.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="argumentsToExclude"/> is <see langword="null"/>.</exception>
        public void AddExcludedArguments(IEnumerable<string> argumentsToExclude)
        {
            if (argumentsToExclude == null)
            {
                throw new ArgumentNullException(nameof(argumentsToExclude), "argumentsToExclude must not be null");
            }

            this.excludedSwitches.AddRange(argumentsToExclude);
        }

        /// <summary>
        /// Adds a path to a packed Chrome extension (.crx file) to the list of extensions
        /// to be installed in the instance of Chrome.
        /// </summary>
        /// <param name="pathToExtension">The full path to the extension to add.</param>
        /// <exception cref="ArgumentException">If <paramref name="pathToExtension"/> is <see langword="null"/> or <see cref="string.Empty"/>.</exception>
        public void AddExtension(string pathToExtension)
        {
            if (string.IsNullOrEmpty(pathToExtension))
            {
                throw new ArgumentException("pathToExtension must not be null or empty", nameof(pathToExtension));
            }

            this.AddExtensions(pathToExtension);
        }

        /// <summary>
        /// Adds a list of paths to packed Chrome extensions (.crx files) to be installed
        /// in the instance of Chrome.
        /// </summary>
        /// <param name="extensions">An array of full paths to the extensions to add.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="extensions"/> is <see langword="null"/>.</exception>
        /// <exception cref="FileNotFoundException">If any extension file path does not point to a file.</exception>
        public void AddExtensions(params string[] extensions)
        {
            this.AddExtensions((IEnumerable<string>)extensions);
        }

        /// <summary>
        /// Adds a list of paths to packed Chrome extensions (.crx files) to be installed
        /// in the instance of Chrome.
        /// </summary>
        /// <param name="extensions">An <see cref="IEnumerable{T}"/> of full paths to the extensions to add.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="extensions"/> is <see langword="null"/>.</exception>
        /// <exception cref="FileNotFoundException">If any extension file path does not point to a file.</exception>
        public void AddExtensions(IEnumerable<string> extensions)
        {
            if (extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions), "extensions must not be null");
            }

            foreach (string extension in extensions)
            {
                if (!File.Exists(extension))
                {
                    throw new FileNotFoundException("No extension found at the specified path", extension);
                }

                this.extensionFiles.Add(extension);
            }
        }

        /// <summary>
        /// Adds a base64-encoded string representing a Chrome extension to the list of extensions
        /// to be installed in the instance of Chrome.
        /// </summary>
        /// <param name="extension">A base64-encoded string representing the extension to add.</param>
        /// <exception cref="ArgumentException">If <paramref name="extension"/> is <see langword="null"/> or <see cref="string.Empty"/>.</exception>
        /// <exception cref="WebDriverException">If the extension string is not valid base-64.</exception>
        public void AddEncodedExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                throw new ArgumentException("extension must not be null or empty", nameof(extension));
            }

            this.AddEncodedExtensions(extension);
        }

        /// <summary>
        /// Adds a list of base64-encoded strings representing Chrome extensions to the list of extensions
        /// to be installed in the instance of Chrome.
        /// </summary>
        /// <param name="extensions">An array of base64-encoded strings representing the extensions to add.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="extensions"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebDriverException">If an extension string is not valid base-64.</exception>
        public void AddEncodedExtensions(params string[] extensions)
        {
            this.AddEncodedExtensions((IEnumerable<string>)extensions);
        }

        /// <summary>
        /// Adds a list of base64-encoded strings representing Chrome extensions to be installed
        /// in the instance of Chrome.
        /// </summary>
        /// <param name="extensions">An <see cref="IEnumerable{T}"/> of base64-encoded strings
        /// representing the extensions to add.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="extensions"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebDriverException">If an extension string is not valid base-64.</exception>
        public void AddEncodedExtensions(IEnumerable<string> extensions)
        {
            if (extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions), "extensions must not be null");
            }

            foreach (string extension in extensions)
            {
                // Run the extension through the base64 converter to test that the
                // string is not malformed.
                try
                {
                    Convert.FromBase64String(extension);
                }
                catch (FormatException ex)
                {
                    throw new WebDriverException("Could not properly decode the base64 string", ex);
                }

                this.encodedExtensions.Add(extension);
            }
        }

        /// <summary>
        /// Adds a preference for the user-specific profile or "user data directory."
        /// If the specified preference already exists, it will be overwritten.
        /// </summary>
        /// <param name="preferenceName">The name of the preference to set.</param>
        /// <param name="preferenceValue">The value of the preference to set.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="preferenceName"/> is <see langword="null"/>.</exception>
        public void AddUserProfilePreference(string preferenceName, object preferenceValue)
        {
            if (this.userProfilePreferences == null)
            {
                this.userProfilePreferences = new Dictionary<string, object>();
            }

            this.userProfilePreferences[preferenceName] = preferenceValue;
        }

        /// <summary>
        /// Adds a preference for the local state file in the user's data directory for Chromium.
        /// If the specified preference already exists, it will be overwritten.
        /// </summary>
        /// <param name="preferenceName">The name of the preference to set.</param>
        /// <param name="preferenceValue">The value of the preference to set.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="preferenceName"/> is <see langword="null"/>.</exception>
        public void AddLocalStatePreference(string preferenceName, object preferenceValue)
        {
            if (this.localStatePreferences == null)
            {
                this.localStatePreferences = new Dictionary<string, object>();
            }

            this.localStatePreferences[preferenceName] = preferenceValue;
        }

        /// <summary>
        /// Allows the Chromium browser to emulate a mobile device.
        /// </summary>
        /// <param name="deviceName">The name of the device to emulate. The device name must be a
        /// valid device name from the Chrome DevTools Emulation panel.</param>
        /// <remarks>Specifying an invalid device name will not throw an exeption, but
        /// will generate an error in Chrome when the driver starts. To unset mobile
        /// emulation, call this method with <see langword="null"/> as the argument.</remarks>
        public void EnableMobileEmulation(string? deviceName)
        {
            this.mobileEmulationDeviceSettings = null;
            this.mobileEmulationDeviceName = deviceName;
        }

        /// <summary>
        /// Allows the Chromium browser to emulate a mobile device.
        /// </summary>
        /// <param name="deviceSettings">The <see cref="ChromiumMobileEmulationDeviceSettings"/>
        /// object containing the settings of the device to emulate.</param>
        /// <exception cref="ArgumentException">Thrown if the device settings option does
        /// not have a user agent string set.</exception>
        /// <remarks>Specifying an invalid device name will not throw an exeption, but
        /// will generate an error in Chrome when the driver starts. To unset mobile
        /// emulation, call this method with <see langword="null"/> as the argument.</remarks>
        public void EnableMobileEmulation(ChromiumMobileEmulationDeviceSettings? deviceSettings)
        {
            this.mobileEmulationDeviceName = null;
            if (deviceSettings != null && string.IsNullOrEmpty(deviceSettings.UserAgent))
            {
                throw new ArgumentException("Device settings must include a user agent string.", nameof(deviceSettings));
            }

            this.mobileEmulationDeviceSettings = deviceSettings;
        }

        /// <summary>
        /// Adds a type of window that will be listed in the list of window handles
        /// returned by the Chrome driver.
        /// </summary>
        /// <param name="windowType">The name of the window type to add.</param>
        /// <remarks>This method can be used to allow the driver to access {webview}
        /// elements by adding "webview" as a window type.</remarks>
        /// <exception cref="ArgumentException">If <paramref name="windowType"/> is <see langword="null"/> or <see cref="string.Empty"/>.</exception>
        public void AddWindowType(string windowType)
        {
            if (string.IsNullOrEmpty(windowType))
            {
                throw new ArgumentException("windowType must not be null or empty", nameof(windowType));
            }

            this.AddWindowTypes(windowType);
        }

        /// <summary>
        /// Adds a list of window types that will be listed in the list of window handles
        /// returned by the Chromium driver.
        /// </summary>
        /// <param name="windowTypesToAdd">An array of window types to add.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="windowTypesToAdd"/> is <see langword="null"/>.</exception>
        public void AddWindowTypes(params string[] windowTypesToAdd)
        {
            this.AddWindowTypes((IEnumerable<string>)windowTypesToAdd);
        }

        /// <summary>
        /// Adds a list of window types that will be listed in the list of window handles
        /// returned by the Chromium driver.
        /// </summary>
        /// <param name="windowTypesToAdd">An <see cref="IEnumerable{T}"/> of window types to add.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="windowTypesToAdd"/> is <see langword="null"/>.</exception>
        public void AddWindowTypes(IEnumerable<string> windowTypesToAdd)
        {
            if (windowTypesToAdd == null)
            {
                throw new ArgumentNullException(nameof(windowTypesToAdd), "windowTypesToAdd must not be null");
            }

            this.windowTypes.AddRange(windowTypesToAdd);
        }

        /// <summary>
        /// Provides a means to add additional capabilities not yet added as type safe options
        /// for the Chromium driver.
        /// </summary>
        /// <param name="optionName">The name of the capability to add.</param>
        /// <param name="optionValue">The value of the capability to add.</param>
        /// <exception cref="ArgumentException">
        /// thrown when attempting to add a capability for which there is already a type safe option, or
        /// when <paramref name="optionName"/> is <see langword="null"/> or the empty string.
        /// </exception>
        /// <remarks>Calling <see cref="AddAdditionalChromiumOption(string, object)"/>
        /// where <paramref name="optionName"/> has already been added will overwrite the
        /// existing value with the new value in <paramref name="optionValue"/>.
        /// Calling this method adds capabilities to the Chromium-specific options object passed to
        /// webdriver executable (e.g. property name 'goog:chromeOptions').</remarks>
        /// <exception cref="ArgumentException">
        /// thrown when attempting to add a capability for which there is already a type safe option, or
        /// when <paramref name="optionName"/> is <see langword="null"/> or the empty string.
        /// </exception>
        protected void AddAdditionalChromiumOption(string optionName, object optionValue)
        {
            this.ValidateCapabilityName(optionName);
            this.additionalChromeOptions[optionName] = optionValue;
        }

        /// <summary>
        /// Returns DesiredCapabilities for Chromium with these options included as
        /// capabilities. This does not copy the options. Further changes will be
        /// reflected in the returned capabilities.
        /// </summary>
        /// <returns>The DesiredCapabilities for Chrome with these options.</returns>
        public override ICapabilities ToCapabilities()
        {
            Dictionary<string, object> chromeOptions = this.BuildChromeOptionsDictionary();

            IWritableCapabilities capabilities = this.GenerateDesiredCapabilities(false);
            capabilities.SetCapability(this.CapabilityName, chromeOptions);

            AddVendorSpecificChromiumCapabilities(capabilities);

            Dictionary<string, object>? loggingPreferences = this.GenerateLoggingPreferencesDictionary();
            if (loggingPreferences != null)
            {
                capabilities.SetCapability(LoggingPreferencesChromeOption, loggingPreferences);
            }

            return capabilities.AsReadOnly();
        }

        /// <summary>
        /// Adds vendor-specific capabilities for Chromium-based browsers.
        /// </summary>
        /// <param name="capabilities">The capabilities to add.</param>
        protected virtual void AddVendorSpecificChromiumCapabilities(IWritableCapabilities capabilities)
        {
        }

        private Dictionary<string, object> BuildChromeOptionsDictionary()
        {
            Dictionary<string, object> chromeOptions = new Dictionary<string, object>();
            if (this.Arguments.Count > 0)
            {
                chromeOptions[ArgumentsChromeOption] = this.Arguments;
            }

            if (!string.IsNullOrEmpty(this.BinaryLocation))
            {
                chromeOptions[BinaryChromeOption] = this.BinaryLocation!;
            }

            ReadOnlyCollection<string> extensions = this.Extensions;
            if (extensions.Count > 0)
            {
                chromeOptions[ExtensionsChromeOption] = extensions;
            }

            if (this.localStatePreferences != null && this.localStatePreferences.Count > 0)
            {
                chromeOptions[LocalStateChromeOption] = this.localStatePreferences;
            }

            if (this.userProfilePreferences != null && this.userProfilePreferences.Count > 0)
            {
                chromeOptions[PreferencesChromeOption] = this.userProfilePreferences;
            }

            if (this.LeaveBrowserRunning)
            {
                chromeOptions[DetachChromeOption] = this.LeaveBrowserRunning;
            }

            if (!this.useSpecCompliantProtocol)
            {
                chromeOptions[UseSpecCompliantProtocolOption] = this.useSpecCompliantProtocol;
            }

            if (!string.IsNullOrEmpty(this.DebuggerAddress))
            {
                chromeOptions[DebuggerAddressChromeOption] = this.DebuggerAddress!;
            }

            if (this.excludedSwitches.Count > 0)
            {
                chromeOptions[ExcludeSwitchesChromeOption] = this.excludedSwitches;
            }

            if (!string.IsNullOrEmpty(this.MinidumpPath))
            {
                chromeOptions[MinidumpPathChromeOption] = this.MinidumpPath!;
            }

            if (!string.IsNullOrEmpty(this.mobileEmulationDeviceName) || this.mobileEmulationDeviceSettings != null)
            {
                chromeOptions[MobileEmulationChromeOption] = GenerateMobileEmulationSettingsDictionary(this.mobileEmulationDeviceSettings, this.mobileEmulationDeviceName);
            }

            if (this.PerformanceLoggingPreferences != null)
            {
                chromeOptions[PerformanceLoggingPreferencesChromeOption] = GeneratePerformanceLoggingPreferencesDictionary(this.PerformanceLoggingPreferences);
            }

            if (this.AndroidOptions != null)
            {
                AddAndroidOptions(chromeOptions, this.AndroidOptions);
            }

            if (this.windowTypes.Count > 0)
            {
                chromeOptions[WindowTypesChromeOption] = this.windowTypes;
            }

            foreach (KeyValuePair<string, object> pair in this.additionalChromeOptions)
            {
                chromeOptions.Add(pair.Key, pair.Value);
            }

            return chromeOptions;
        }

        private static void AddAndroidOptions(Dictionary<string, object> chromeOptions, ChromiumAndroidOptions androidOptions)
        {
            chromeOptions["androidPackage"] = androidOptions.AndroidPackage;

            if (!string.IsNullOrEmpty(androidOptions.AndroidDeviceSerial))
            {
                chromeOptions["androidDeviceSerial"] = androidOptions.AndroidDeviceSerial!;
            }

            if (!string.IsNullOrEmpty(androidOptions.AndroidActivity))
            {
                chromeOptions["androidActivity"] = androidOptions.AndroidActivity!;
            }

            if (!string.IsNullOrEmpty(androidOptions.AndroidProcess))
            {
                chromeOptions["androidProcess"] = androidOptions.AndroidProcess!;
            }

            if (androidOptions.UseRunningApp)
            {
                chromeOptions["androidUseRunningApp"] = androidOptions.UseRunningApp;
            }
        }

        private static Dictionary<string, object> GeneratePerformanceLoggingPreferencesDictionary(ChromiumPerformanceLoggingPreferences prefs)
        {
            Dictionary<string, object> perfLoggingPrefsDictionary = new Dictionary<string, object>();
            perfLoggingPrefsDictionary["enableNetwork"] = prefs.IsCollectingNetworkEvents;
            perfLoggingPrefsDictionary["enablePage"] = prefs.IsCollectingPageEvents;

            string tracingCategories = prefs.TracingCategories;
            if (!string.IsNullOrEmpty(tracingCategories))
            {
                perfLoggingPrefsDictionary["traceCategories"] = tracingCategories;
            }

            perfLoggingPrefsDictionary["bufferUsageReportingInterval"] = Convert.ToInt64(prefs.BufferUsageReportingInterval.TotalMilliseconds);

            return perfLoggingPrefsDictionary;
        }

        private static Dictionary<string, object?> GenerateMobileEmulationSettingsDictionary(ChromiumMobileEmulationDeviceSettings? settings, string? deviceName)
        {
            Dictionary<string, object?> mobileEmulationSettings = new Dictionary<string, object?>();

            if (!string.IsNullOrEmpty(deviceName))
            {
                mobileEmulationSettings["deviceName"] = deviceName!;
            }
            else if (settings != null)
            {
                mobileEmulationSettings["userAgent"] = settings.UserAgent;
                Dictionary<string, object> deviceMetrics = new Dictionary<string, object>();
                deviceMetrics["width"] = settings.Width;
                deviceMetrics["height"] = settings.Height;
                deviceMetrics["pixelRatio"] = settings.PixelRatio;
                if (!settings.EnableTouchEvents)
                {
                    deviceMetrics["touch"] = settings.EnableTouchEvents;
                }

                mobileEmulationSettings["deviceMetrics"] = deviceMetrics;
            }

            return mobileEmulationSettings;
        }
    }
}
