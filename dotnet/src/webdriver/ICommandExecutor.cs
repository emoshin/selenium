// <copyright file="ICommandExecutor.cs" company="Selenium Committers">
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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace OpenQA.Selenium
{
    /// <summary>
    /// Provides a way to send commands to the remote server
    /// </summary>
    public interface ICommandExecutor : IDisposable
    {
        /// <summary>
        /// Attempts to add a command to the repository of commands known to this executor.
        /// </summary>
        /// <param name="commandName">The name of the command to attempt to add.</param>
        /// <param name="info">The <see cref="CommandInfo"/> describing the command to add.</param>
        /// <returns><see langword="true"/> if the new command has been added successfully; otherwise, <see langword="false"/>.</returns>
        bool TryAddCommand(string commandName, [NotNullWhen(true)] CommandInfo? info);

        /// <summary>
        /// Executes a command
        /// </summary>
        /// <param name="commandToExecute">The command you wish to execute</param>
        /// <returns>A response from the browser</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="commandToExecute"/> is <see langword="null"/>.</exception>
        Response Execute(Command commandToExecute);


        /// <summary>
        /// Executes a command as an asynchronous task.
        /// </summary>
        /// <param name="commandToExecute">The command you wish to execute</param>
        /// <returns>A task object representing the asynchronous operation</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="commandToExecute"/> is <see langword="null"/>.</exception>
        Task<Response> ExecuteAsync(Command commandToExecute);
    }
}
