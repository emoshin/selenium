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

'use strict'

const assert = require('node:assert')
const net = require('node:net')
const portprober = require('selenium-webdriver/net/portprober')
const host = '127.0.0.1'

describe('isFree', function () {
  let server

  beforeEach(function () {
    server = net.createServer(function () {})
  })

  afterEach(function (done) {
    if (!server) return done()
    server.close(function () {
      done()
    })
  })

  it('should work for INADDR_ANY', function (done) {
    server.listen(0, function () {
      const port = server.address().port
      assertPortNotFree(port)
        .then(function () {
          return new Promise((resolve) => {
            server.close(function () {
              server = null
              resolve(assertPortIsFree(port))
            })
          })
        })
        .then(function () {
          done()
        }, done)
    })
  })

  it('should work for a specific host', function (done) {
    server.listen(0, host, function () {
      const port = server.address().port
      assertPortNotFree(port, host)
        .then(function () {
          return new Promise((resolve) => {
            server.close(function () {
              server = null
              resolve(assertPortIsFree(port, host))
            })
          })
        })
        .then(function () {
          done()
        }, done)
    })
  })
})

describe('findFreePort', function () {
  let server

  beforeEach(function () {
    server = net.createServer(function () {})
  })

  afterEach(function (done) {
    if (!server) return done()
    server.close(function () {
      done()
    })
  })

  it('should work for INADDR_ANY', function (done) {
    portprober.findFreePort().then(function (port) {
      server.listen(port, function () {
        assertPortNotFree(port)
          .then(function () {
            return new Promise((resolve) => {
              server.close(function () {
                server = null
                resolve(assertPortIsFree(port))
              })
            })
          })
          .then(function () {
            done()
          }, done)
      })
    })
  })

  it('should work for a specific host', function (done) {
    portprober.findFreePort(host).then(function (port) {
      server.listen(port, host, function () {
        assertPortNotFree(port, host)
          .then(function () {
            return new Promise((resolve) => {
              server.close(function () {
                server = null
                resolve(assertPortIsFree(port, host))
              })
            })
          })
          .then(function () {
            done()
          }, done)
      })
    })
  })
})

function assertPortIsFree(port, opt_host) {
  return portprober.isFree(port, opt_host).then(function (free) {
    assert.ok(free, 'port should be free')
  })
}

function assertPortNotFree(port, opt_host) {
  return portprober.isFree(port, opt_host).then(function (free) {
    assert.ok(!free, 'port should is not free')
  })
}
