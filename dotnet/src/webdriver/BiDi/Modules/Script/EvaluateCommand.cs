// <copyright file="EvaluateCommand.cs" company="Selenium Committers">
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

namespace OpenQA.Selenium.BiDi.Modules.Script;

internal class EvaluateCommand(EvaluateCommandParameters @params)
    : Command<EvaluateCommandParameters>(@params, "script.evaluate");

internal record EvaluateCommandParameters(string Expression, Target Target, bool AwaitPromise, ResultOwnership? ResultOwnership, SerializationOptions? SerializationOptions, bool? UserActivation) : CommandParameters;

public record EvaluateOptions : CommandOptions
{
    public ResultOwnership? ResultOwnership { get; set; }

    public SerializationOptions? SerializationOptions { get; set; }

    public bool? UserActivation { get; set; }
}

// https://github.com/dotnet/runtime/issues/72604
//[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
//[JsonDerivedType(typeof(EvaluateResultSuccess), "success")]
//[JsonDerivedType(typeof(EvaluateResultException), "exception")]
public abstract record EvaluateResult;

public record EvaluateResultSuccess(RemoteValue Result, Realm Realm) : EvaluateResult
{
    public static implicit operator RemoteValue(EvaluateResultSuccess success) => success.Result;
}

public record EvaluateResultException(ExceptionDetails ExceptionDetails, Realm Realm) : EvaluateResult;

public record ExceptionDetails(long ColumnNumber, long LineNumber, StackTrace StackTrace, string Text);
