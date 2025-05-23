// <copyright file="LocalFileDetector.cs" company="Selenium Committers">
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

using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace OpenQA.Selenium.Remote
{
    /// <summary>
    /// Represents a file detector for determining whether a file
    /// must be uploaded to a remote server.
    /// </summary>
    public class LocalFileDetector : IFileDetector
    {
        /// <summary>
        /// Returns a value indicating whether a specified key sequence represents
        /// a file name and path.
        /// </summary>
        /// <param name="keySequence">The sequence to test for file existence.</param>
        /// <returns><see langword="true"/> if the key sequence represents a file; otherwise, <see langword="false"/>.</returns>
        public bool IsFile([NotNullWhen(true)] string? keySequence)
        {
            return File.Exists(keySequence);
        }
    }
}
