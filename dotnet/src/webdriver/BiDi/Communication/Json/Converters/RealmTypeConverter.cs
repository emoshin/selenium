// <copyright file="RealmTypeConverter.cs" company="Selenium Committers">
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

using OpenQA.Selenium.BiDi.Modules.Script;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenQA.Selenium.BiDi.Communication.Json.Converters;

internal class RealmTypeConverter : JsonConverter<RealmType>
{
    public override RealmType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var realmType = reader.GetString();

        return realmType switch
        {
            "window" => RealmType.Window,
            "dedicated-worker" => RealmType.DedicatedWorker,
            "shared-worker" => RealmType.SharedWorker,
            "service-worker" => RealmType.ServiceWorker,
            "worker" => RealmType.Worker,
            "paint-worker" => RealmType.PaintWorker,
            "audio-worker" => RealmType.AudioWorker,
            "worklet" => RealmType.Worklet,
            _ => throw new JsonException($"Unrecognized '{realmType}' value of {typeof(RealmType)}."),
        };
    }

    public override void Write(Utf8JsonWriter writer, RealmType value, JsonSerializerOptions options)
    {
        var str = value switch
        {
            RealmType.Window => "window",
            RealmType.DedicatedWorker => "dedicated-worker",
            RealmType.SharedWorker => "shared-worker",
            RealmType.ServiceWorker => "service-worker",
            RealmType.Worker => "worker",
            RealmType.PaintWorker => "paint-worker",
            RealmType.AudioWorker => "audio-worker",
            RealmType.Worklet => "worklet",
            _ => throw new JsonException($"Unrecognized '{value}' value of {typeof(RealmType)}."),
        };

        writer.WriteStringValue(str);
    }
}
