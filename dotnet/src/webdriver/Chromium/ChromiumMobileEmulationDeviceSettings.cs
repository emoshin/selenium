// <copyright file="ChromiumMobileEmulationDeviceSettings.cs" company="Selenium Committers">
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

namespace OpenQA.Selenium.Chromium
{
    /// <summary>
    /// Represents the type-safe options for setting settings for emulating a
    /// mobile device in the Chromium browser.
    /// </summary>
    public class ChromiumMobileEmulationDeviceSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChromiumMobileEmulationDeviceSettings"/> class.
        /// </summary>
        public ChromiumMobileEmulationDeviceSettings()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChromiumMobileEmulationDeviceSettings"/> class.
        /// </summary>
        /// <param name="userAgent">The user agent string to be used by the browser when emulating
        /// a mobile device.</param>
        public ChromiumMobileEmulationDeviceSettings(string? userAgent)
        {
            this.UserAgent = userAgent;
        }

        /// <summary>
        /// Gets or sets the user agent string to be used by the browser when emulating
        /// a mobile device.
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Gets or sets the width in pixels to be used by the browser when emulating
        /// a mobile device.
        /// </summary>
        public long Width { get; set; }

        /// <summary>
        /// Gets or sets the height in pixels to be used by the browser when emulating
        /// a mobile device.
        /// </summary>
        public long Height { get; set; }

        /// <summary>
        /// Gets or sets the pixel ratio to be used by the browser when emulating
        /// a mobile device.
        /// </summary>
        public double PixelRatio { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether touch events should be enabled by
        /// the browser when emulating a mobile device. Defaults to <see langword="true"/>.
        /// </summary>
        public bool EnableTouchEvents { get; set; } = true;
    }
}
