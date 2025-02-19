// <copyright file="ICoordinates.cs" company="Selenium Committers">
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

using System.Drawing;

#nullable enable

namespace OpenQA.Selenium.Interactions.Internal
{
    /// <summary>
    /// Provides location of the element using various frames of reference.
    /// </summary>
    public interface ICoordinates
    {
        /// <summary>
        /// Gets the location of an element in absolute screen coordinates.
        /// </summary>
        Point LocationOnScreen { get; }

        /// <summary>
        /// Gets the location of an element relative to the origin of the view port.
        /// </summary>
        Point LocationInViewport { get; }

        /// <summary>
        /// Gets the location of an element's position within the HTML DOM.
        /// </summary>
        Point LocationInDom { get; }

        /// <summary>
        /// Gets a locator providing a user-defined location for this element.
        /// </summary>
        object AuxiliaryLocator { get; }
    }
}
