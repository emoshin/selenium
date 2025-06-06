// <copyright file="UnknownErrorException.cs" company="Selenium Committers">
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

namespace OpenQA.Selenium
{
    /// <summary>
    /// An unknown error occurred in the remote end while processing the command.
    /// </summary>
    [Serializable]
    public class UnknownErrorException : WebDriverException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownErrorException"/> class with the specified message.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        public UnknownErrorException(string? message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownErrorException"/> class with the specified message and inner exception.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="innerException">The inner exception for this exception.</param>
        public UnknownErrorException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
