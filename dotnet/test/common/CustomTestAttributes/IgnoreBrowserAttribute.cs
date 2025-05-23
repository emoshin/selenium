// <copyright file="IgnoreBrowserAttribute.cs" company="Selenium Committers">
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

using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using OpenQA.Selenium.Environment;
using System;
using System.Collections.Generic;

namespace OpenQA.Selenium
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class IgnoreBrowserAttribute : NUnitAttribute, IApplyToTest
    {
        private readonly Browser browser;
        private readonly string ignoreReason = string.Empty;

        public IgnoreBrowserAttribute(Browser browser)
        {
            this.browser = browser;
        }

        public IgnoreBrowserAttribute(Browser browser, string reason)
            : this(browser)
        {
            this.ignoreReason = reason;
        }

        public Browser Value
        {
            get { return browser; }
        }

        public string Reason
        {
            get { return ignoreReason; }
        }

        public void ApplyToTest(Test test)
        {
            if (test.RunState != RunState.NotRunnable)
            {
                List<Attribute> ignoreAttributes = new List<Attribute>();
                if (test.IsSuite)
                {
                    Attribute[] ignoreClassAttributes = test.TypeInfo.GetCustomAttributes<IgnoreBrowserAttribute>(true);
                    if (ignoreClassAttributes.Length > 0)
                    {
                        ignoreAttributes.AddRange(ignoreClassAttributes);
                    }
                }
                else
                {
                    IgnoreBrowserAttribute[] ignoreMethodAttributes = test.Method.GetCustomAttributes<IgnoreBrowserAttribute>(true);
                    if (ignoreMethodAttributes.Length > 0)
                    {
                        ignoreAttributes.AddRange(ignoreMethodAttributes);
                    }
                }

                foreach (Attribute attr in ignoreAttributes)
                {
                    IgnoreBrowserAttribute browserToIgnoreAttr = attr as IgnoreBrowserAttribute;
                    if (browserToIgnoreAttr != null && IgnoreTestForBrowser(browserToIgnoreAttr.Value))
                    {
                        string ignoreReason = "Ignoring browser " + EnvironmentManager.Instance.Browser.ToString() + ".";
                        if (!string.IsNullOrEmpty(browserToIgnoreAttr.Reason))
                        {
                            ignoreReason = ignoreReason + " " + browserToIgnoreAttr.Reason;
                        }

                        test.RunState = RunState.Ignored;
                        test.Properties.Set(PropertyNames.SkipReason, browserToIgnoreAttr.Reason);
                    }
                }
            }
        }

        private bool IgnoreTestForBrowser(Browser browserToIgnore)
        {
            return browserToIgnore.Equals(EnvironmentManager.Instance.Browser) || browserToIgnore.Equals(Browser.All) || IsRemoteInstanceOfBrowser(browserToIgnore);
        }

        private bool IsRemoteInstanceOfBrowser(Browser desiredBrowser)
        {
            bool isRemoteInstance = false;
            switch (desiredBrowser)
            {
                case Browser.IE:
                    if (EnvironmentManager.Instance.RemoteCapabilities == "internet explorer")
                    {
                        isRemoteInstance = true;
                    }
                    break;

                case Browser.Firefox:
                    if (EnvironmentManager.Instance.RemoteCapabilities == "firefox")
                    {
                        isRemoteInstance = true;
                    }
                    break;

                case Browser.Chrome:
                    if (EnvironmentManager.Instance.RemoteCapabilities == "chrome")
                    {
                        isRemoteInstance = true;
                    }
                    break;
                case Browser.Edge:
                    if (EnvironmentManager.Instance.RemoteCapabilities == "MicrosoftEdge")
                    {
                        isRemoteInstance = true;
                    }
                    break;
            }
            return isRemoteInstance;
        }
    }
}
