// <copyright file="PreloadScript.cs" company="Selenium Committers">
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
using System.Threading.Tasks;

namespace OpenQA.Selenium.BiDi.Modules.Script;

public class PreloadScript : IAsyncDisposable
{
    private readonly BiDi _bidi;

    public PreloadScript(BiDi bidi, string id)
    {
        _bidi = bidi;
        Id = id;
    }

    public string Id { get; }

    public Task RemoveAsync()
    {
        return _bidi.Script.RemovePreloadScriptAsync(this);
    }

    public async ValueTask DisposeAsync()
    {
        await RemoveAsync().ConfigureAwait(false);
    }
}
