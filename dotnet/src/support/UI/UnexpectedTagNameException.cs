// <copyright file="UnexpectedTagNameException.cs" company="Selenium Committers">
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
using System.Globalization;

namespace OpenQA.Selenium.Support.UI
{
    /// <summary>
    /// The exception thrown when using the Select class on a tag that
    /// does not support the HTML select element's selection semantics.
    /// </summary>
    [Serializable]
    public class UnexpectedTagNameException : WebDriverException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedTagNameException"/> class with
        /// the expected tag name and the actual tag name.
        /// </summary>
        /// <param name="expected">The tag name that was expected.</param>
        /// <param name="actual">The actual tag name of the element.</param>
        public UnexpectedTagNameException(string expected, string actual)
            : base(string.Format(CultureInfo.InvariantCulture, "Element should have been {0} but was {1}", expected, actual))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedTagNameException"/> class.
        /// </summary>
        public UnexpectedTagNameException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedTagNameException"/> class with
        /// a specified error message.
        /// </summary>
        /// <param name="message">The message of the exception</param>
        public UnexpectedTagNameException(string? message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedTagNameException"/> class with
        /// a specified error message and a reference to the inner exception that is the
        /// cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception,
        /// or <see langword="null"/> if no inner exception is specified.</param>
        public UnexpectedTagNameException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
