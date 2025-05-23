// <copyright file="ElementCoordinates.cs" company="Selenium Committers">
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

using OpenQA.Selenium.Interactions.Internal;
using OpenQA.Selenium.Internal;
using System;

namespace OpenQA.Selenium
{
    /// <summary>
    /// Defines the interface through which the user can discover where an element is on the screen.
    /// </summary>
    internal sealed class ElementCoordinates : ICoordinates
    {
        private readonly WebElement element;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementCoordinates"/> class.
        /// </summary>
        /// <param name="element">The <see cref="WebElement"/> to be located.</param>
        public ElementCoordinates(WebElement element)
        {
            this.element = element ?? throw new ArgumentNullException(nameof(element));
        }

        /// <summary>
        /// Gets the location of an element in absolute screen coordinates.
        /// </summary>
        public System.Drawing.Point LocationOnScreen => throw new NotImplementedException();

        /// <summary>
        /// Gets the location of an element relative to the origin of the view port.
        /// </summary>
        public System.Drawing.Point LocationInViewport => this.element.LocationOnScreenOnceScrolledIntoView;

        /// <summary>
        /// Gets the location of an element's position within the HTML DOM.
        /// </summary>
        public System.Drawing.Point LocationInDom => this.element.Location;

        /// <summary>
        /// Gets a locator providing a user-defined location for this element.
        /// </summary>
        public object AuxiliaryLocator
        {
            get
            {
                // Note that the OSS dialect of the wire protocol for the Actions API
                // uses the raw ID of the element, not an element reference. To use this,
                // extract the ID using the well-known key to the dictionary for element
                // references.
                return ((IWebDriverObjectReference)this.element).ObjectReferenceId;
            }
        }
    }
}
