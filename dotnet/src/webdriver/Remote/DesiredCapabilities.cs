// <copyright file="DesiredCapabilities.cs" company="Selenium Committers">
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

using OpenQA.Selenium.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace OpenQA.Selenium.Remote
{
    /// <summary>
    /// Internal class to specify the requested capabilities of the browser for <see cref="IWebDriver"/>.
    /// </summary>
    internal class DesiredCapabilities : IWritableCapabilities, IHasCapabilitiesDictionary
    {
        private readonly Dictionary<string, object> capabilities = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DesiredCapabilities"/> class
        /// </summary>
        /// <param name="browser">Name of the browser e.g. firefox, internet explorer, safari</param>
        /// <param name="version">Version of the browser</param>
        /// <param name="platform">The platform it works on</param>
        public DesiredCapabilities(string browser, string version, Platform platform)
        {
            this.SetCapability(CapabilityType.BrowserName, browser);
            this.SetCapability(CapabilityType.Version, version);
            this.SetCapability(CapabilityType.Platform, platform);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DesiredCapabilities"/> class
        /// </summary>
        public DesiredCapabilities()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DesiredCapabilities"/> class
        /// </summary>
        /// <param name="rawMap">Dictionary of items for the remote driver</param>
        /// <example>
        /// <code>
        /// DesiredCapabilities capabilities = new DesiredCapabilities(new Dictionary<![CDATA[<string,object>]]>(){["browserName","firefox"],["version",string.Empty],["javaScript",true]});
        /// </code>
        /// </example>
        public DesiredCapabilities(Dictionary<string, object>? rawMap)
        {
            if (rawMap != null)
            {
                foreach (KeyValuePair<string, object> entry in rawMap)
                {
                    if (entry.Key == CapabilityType.Platform)
                    {
                        if (entry.Value is string rawAsString)
                        {
                            this.SetCapability(CapabilityType.Platform, Platform.FromString(rawAsString));
                        }
                        else if (entry.Value is Platform rawAsPlatform)
                        {
                            this.SetCapability(CapabilityType.Platform, rawAsPlatform);
                        }
                    }
                    else
                    {
                        this.SetCapability(entry.Key, entry.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DesiredCapabilities"/> class
        /// </summary>
        /// <param name="browser">Name of the browser e.g. firefox, internet explorer, safari</param>
        /// <param name="version">Version of the browser</param>
        /// <param name="platform">The platform it works on</param>
        /// <param name="isSpecCompliant">Sets a value indicating whether the capabilities are
        /// compliant with the W3C WebDriver specification.</param>
        internal DesiredCapabilities(string browser, string version, Platform platform, bool isSpecCompliant)
        {
            this.SetCapability(CapabilityType.BrowserName, browser);
            this.SetCapability(CapabilityType.Version, version);
            this.SetCapability(CapabilityType.Platform, platform);
        }

        /// <summary>
        /// Gets the browser name
        /// </summary>
        public string BrowserName => this.GetCapability(CapabilityType.BrowserName)?.ToString() ?? string.Empty;

        /// <summary>
        /// Gets or sets the platform
        /// </summary>
        public Platform Platform
        {
            get => this.GetCapability(CapabilityType.Platform) as Platform ?? new Platform(PlatformType.Any);
            set => this.SetCapability(CapabilityType.Platform, value);
        }

        /// <summary>
        /// Gets the browser version
        /// </summary>
        public string Version => this.GetCapability(CapabilityType.Version)?.ToString() ?? string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the browser accepts SSL certificates.
        /// </summary>
        public bool AcceptInsecureCerts
        {
            get
            {
                bool acceptSSLCerts = false;
                object? capabilityValue = this.GetCapability(CapabilityType.AcceptInsecureCertificates);
                if (capabilityValue != null)
                {
                    acceptSSLCerts = (bool)capabilityValue;
                }

                return acceptSSLCerts;
            }

            set => this.SetCapability(CapabilityType.AcceptInsecureCertificates, value);
        }

        /// <summary>
        /// Gets the underlying Dictionary for a given set of capabilities.
        /// </summary>
        IDictionary<string, object> IHasCapabilitiesDictionary.CapabilitiesDictionary => this.CapabilitiesDictionary;

        /// <summary>
        /// Gets the underlying Dictionary for a given set of capabilities.
        /// </summary>
        internal IDictionary<string, object> CapabilitiesDictionary => new ReadOnlyDictionary<string, object>(this.capabilities);

        /// <summary>
        /// Gets the capability value with the specified name.
        /// </summary>
        /// <param name="capabilityName">The name of the capability to get.</param>
        /// <returns>The value of the capability.</returns>
        /// <exception cref="ArgumentException">
        /// The specified capability name is not in the set of capabilities.
        /// </exception>
        public object this[string capabilityName]
        {
            get
            {
                if (!this.capabilities.TryGetValue(capabilityName, out object? capabilityValue))
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The capability {0} is not present in this set of capabilities", capabilityName));
                }

                return capabilityValue;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the browser has a given capability.
        /// </summary>
        /// <param name="capability">The capability to get.</param>
        /// <returns>Returns <see langword="true"/> if the browser has the capability; otherwise, <see langword="false"/>.</returns>
        public bool HasCapability(string capability)
        {
            return this.capabilities.ContainsKey(capability);
        }

        /// <summary>
        /// Gets a capability of the browser.
        /// </summary>
        /// <param name="capability">The capability to get.</param>
        /// <returns>An object associated with the capability, or <see langword="null"/>
        /// if the capability is not set on the browser.</returns>
        public object? GetCapability(string capability)
        {
            object? capabilityValue = null;
            if (this.capabilities.TryGetValue(capability, out object? value))
            {
                capabilityValue = value;
                if (capability == CapabilityType.Platform && capabilityValue is string capabilityValueString)
                {
                    capabilityValue = Platform.FromString(capabilityValueString);
                }
            }

            return capabilityValue;
        }

        /// <summary>
        /// Sets a capability of the browser.
        /// </summary>
        /// <param name="capability">The capability to get.</param>
        /// <param name="capabilityValue">The value for the capability.</param>
        public void SetCapability(string capability, object capabilityValue)
        {
            // Handle the special case of Platform objects. These should
            // be stored in the underlying dictionary as their protocol
            // string representation.
            if (capabilityValue is Platform platformCapabilityValue)
            {
                this.capabilities[capability] = platformCapabilityValue.ProtocolPlatformType;
            }
            else
            {
                this.capabilities[capability] = capabilityValue;
            }
        }

        /// <summary>
        /// Return HashCode for the DesiredCapabilities that has been created
        /// </summary>
        /// <returns>Integer of HashCode generated</returns>
        public override int GetHashCode()
        {
            int result;
            result = this.BrowserName != null ? this.BrowserName.GetHashCode() : 0;
            result = (31 * result) + (this.Version != null ? this.Version.GetHashCode() : 0);
            result = (31 * result) + (this.Platform != null ? this.Platform.GetHashCode() : 0);
            return result;
        }

        /// <summary>
        /// Return a string of capabilities being used
        /// </summary>
        /// <returns>String of capabilities being used</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Capabilities [BrowserName={0}, Platform={1}, Version={2}]", this.BrowserName, this.Platform.PlatformType.ToString(), this.Version);
        }

        /// <summary>
        /// Compare two DesiredCapabilities and will return either true or false
        /// </summary>
        /// <param name="obj">DesiredCapabilities you wish to compare</param>
        /// <returns>true if they are the same or false if they are not</returns>
        public override bool Equals(object? obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (obj is not DesiredCapabilities other)
            {
                return false;
            }

            if (this.BrowserName != null ? this.BrowserName != other.BrowserName : other.BrowserName != null)
            {
                return false;
            }

            if (!this.Platform.IsPlatformType(other.Platform.PlatformType))
            {
                return false;
            }

            if (this.Version != null ? this.Version != other.Version : other.Version != null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a read-only version of this capabilities object.
        /// </summary>
        /// <returns>A read-only version of this capabilities object.</returns>
        public ICapabilities AsReadOnly()
        {
            return new ReadOnlyDesiredCapabilities(this);
        }
    }
}
