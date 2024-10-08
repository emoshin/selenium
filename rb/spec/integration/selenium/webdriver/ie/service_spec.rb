# frozen_string_literal: true

# Licensed to the Software Freedom Conservancy (SFC) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The SFC licenses this file
# to you under the Apache License, Version 2.0 (the
# "License"); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
# KIND, either express or implied.  See the License for the
# specific language governing permissions and limitations
# under the License.

require_relative '../spec_helper'

module Selenium
  module WebDriver
    module IE
      describe Service, exclusive: [{bidi: false, reason: 'Not yet implemented with BiDi'}, {browser: :ie}] do
        let(:service) { described_class.new }
        let(:service_manager) { service.launch }

        after { service_manager.stop }

        it 'auto uses iedriver' do
          service.executable_path = DriverFinder.new(Options.new, described_class.new).driver_path

          expect(service_manager.uri).to be_a(URI)
        end

        it 'can be started outside driver' do
          expect(service_manager.uri).to be_a(URI)
        end
      end
    end # IE
  end # WebDriver
end # Selenium
