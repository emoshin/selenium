// <copyright file="SeleniumManager.cs" company="WebDriver Committers">
// Licensed to the Software Freedom Conservancy (SFC) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The SFC licenses this file
// to you under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;

#if !NET45 && !NET46 && !NET47
using System.Runtime.InteropServices;
#endif

using System.Text;

namespace OpenQA.Selenium
{
    /// <summary>
    /// Wrapper for the Selenium Manager binary.
    /// This implementation is still in beta, and may change.
    /// </summary>
    public static class SeleniumManager
    {
        private static string binary;
        private static readonly List<string> KnownDrivers = new List<string>() {
            "geckodriver",
            "chromedriver",
            "msedgedriver",
            "IEDriverServer"
        };

        /// <summary>
        /// Determines the location of the correct driver.
        /// </summary>
        /// <param name="driverName">Which driver the service needs.</param>
        /// <returns>
        /// The location of the driver.
        /// </returns>
        public static string DriverPath(string driverName)
        {
            driverName = driverName.Replace(".exe", "");
            if (!KnownDrivers.Contains(driverName))
            {
                throw new WebDriverException("Unable to locate driver with name: " + driverName);
            }
            var binaryFile = Binary;
            if (binaryFile == null) return null;

            var arguments = "--driver " + driverName;
            return RunCommand(binaryFile, arguments);
        }

        /// <summary>
        /// Gets the location of the correct Selenium Manager binary.
        /// </summary>
        private static string Binary
        {
            get
            {
                if (string.IsNullOrEmpty(binary))
                {
#if NET45 || NET46 || NET47
                    binary = "selenium-manager/windows/selenium-manager.exe";
#else
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        binary = "selenium-manager/windows/selenium-manager.exe";
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        binary = "selenium-manager/linux/selenium-manager";
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        binary = "selenium-manager/macos/selenium-manager";
                    }
                    else
                    {
                        throw new WebDriverException("Selenium Manager did not find supported operating system");
                    }
#endif
                }

                return binary;
            }
        }

        /// <summary>
        /// Executes a process with the given arguments.
        /// </summary>
        /// <param name="fileName">The path to the Selenium Manager.</param>
        /// <param name="arguments">The switches to be used by Selenium Manager.</param>
        /// <returns>
        /// the standard output of the execution.
        /// </returns>
        private static string RunCommand(string fileName, string arguments)
        {
            Process process = new Process();
            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            StringBuilder outputBuilder = new StringBuilder();
            int processExitCode;

            DataReceivedEventHandler outputHandler = (sender, e) => outputBuilder.AppendLine(e.Data);

            try
            {
                process.OutputDataReceived += outputHandler;
                process.ErrorDataReceived += outputHandler;

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();
            }
            catch (Exception ex)
            {
                throw new WebDriverException($"Error starting process: {fileName} {arguments}", ex);
            }
            finally
            {
                processExitCode = process.ExitCode;
                process.OutputDataReceived -= outputHandler;
                process.ErrorDataReceived -= outputHandler;
            }

            string output = outputBuilder.ToString().Trim();

            if (processExitCode != 0)
            {
                throw new WebDriverException($"Invalid response from process (code {processExitCode}): {fileName} {arguments}\n{output}");
            }

            return output.Replace("INFO\t", "");
        }
    }
}
