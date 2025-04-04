// <copyright file="UserAgent.cs" company="Selenium Committers">
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

namespace OpenQA.Selenium.DevTools
{
    /// <summary>
    /// Represents a user agent string.
    /// </summary>
    public class UserAgent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserAgent"/> type.
        /// </summary>
        [Obsolete("Use the constructor which sets the userAgentString")]
        public UserAgent()
        {
            UserAgentString = null!;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAgent"/> type.
        /// </summary>
        /// <param name="userAgentString">The user agent string.</param>
        public UserAgent(string userAgentString)
        {
            UserAgentString = userAgentString;
        }

        /// <summary>
        /// Gets or sets the user agent string.
        /// </summary>
        public string UserAgentString { get; set; }

        /// <summary>
        /// Gets or sets the language to accept in headers.
        /// </summary>
        public string? AcceptLanguage { get; set; }

        /// <summary>
        /// Gets or sets the value of the platform.
        /// </summary>
        public string? Platform { get; set; }
    }
}
