// <copyright file="CreateCommand.cs" company="Selenium Committers">
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

using OpenQA.Selenium.BiDi.Communication;

namespace OpenQA.Selenium.BiDi.Modules.BrowsingContext;

internal class CreateCommand(CreateCommandParameters @params)
    : Command<CreateCommandParameters>(@params, "browsingContext.create");

internal record CreateCommandParameters(ContextType Type, BrowsingContext? ReferenceContext, bool? Background, Browser.UserContext? UserContext) : CommandParameters;

public record CreateOptions : CommandOptions
{
    public BrowsingContext? ReferenceContext { get; set; }

    public bool? Background { get; set; }

    public Browser.UserContext? UserContext { get; set; }
}

public enum ContextType
{
    Tab,
    Window
}

public record CreateResult(BrowsingContext Context);
