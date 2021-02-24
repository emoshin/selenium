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

package org.openqa.selenium.grid.node.config;

import com.google.common.collect.HashMultimap;
import com.google.common.collect.ImmutableMap;
import com.google.common.collect.ImmutableMultimap;
import com.google.common.collect.Multimap;
import org.openqa.selenium.Capabilities;
import org.openqa.selenium.SessionNotCreatedException;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebDriverInfo;
import org.openqa.selenium.grid.config.Config;
import org.openqa.selenium.grid.config.ConfigException;
import org.openqa.selenium.grid.node.Node;
import org.openqa.selenium.grid.node.SessionFactory;
import org.openqa.selenium.internal.Require;
import org.openqa.selenium.json.Json;
import org.openqa.selenium.json.JsonOutput;
import org.openqa.selenium.remote.service.DriverService;

import java.lang.reflect.Method;
import java.lang.reflect.Modifier;
import java.net.URI;
import java.net.URISyntaxException;
import java.time.Duration;
import java.util.ArrayList;
import java.util.Collection;
import java.util.Comparator;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import java.util.ServiceLoader;
import java.util.function.Function;
import java.util.logging.Logger;
import java.util.stream.Collectors;
import java.util.stream.IntStream;
import java.util.stream.StreamSupport;

public class NodeOptions {

  private static final String NODE_SECTION = "node";

  private static final Logger LOG = Logger.getLogger(NodeOptions.class.getName());
  private static final Json JSON = new Json();
  private static final String DEFAULT_IMPL = "org.openqa.selenium.grid.node.local.LocalNodeFactory";

  private final Config config;

  public NodeOptions(Config config) {
    this.config = Require.nonNull("Config", config);
  }

  public Optional<URI> getPublicGridUri() {
    return config.get(NODE_SECTION, "grid-url").map(url -> {
      try {
        return new URI(url);
      } catch (URISyntaxException e) {
        throw new ConfigException("Unable to construct public URL: " + url);
      }
    });
  }

  public Node getNode() {
    return config.getClass(NODE_SECTION, "implementation", Node.class, DEFAULT_IMPL);
  }

  public Duration getRegisterCycle() {
    // If the user sets 0 or less, we default to 1s.
    int seconds = Math.max(config.getInt(NODE_SECTION, "register-cycle").orElse(10), 1);
    return Duration.ofSeconds(seconds);
  }

  public Duration getRegisterPeriod() {
    // If the user sets 0 or less, we default to 1s.
    int seconds = Math.max(config.getInt(NODE_SECTION, "register-period").orElse(120), 1);
    return Duration.ofSeconds(seconds);
  }

  public Duration getHeartbeatPeriod() {
    // If the user sets 0 or less, we default to 1s.
    int seconds = Math.max(config.getInt(NODE_SECTION, "heartbeat-period").orElse(10), 1);
    return Duration.ofSeconds(seconds);
  }

  public Map<Capabilities, Collection<SessionFactory>> getSessionFactories(
    /* Danger! Java stereotype ahead! */
    Function<Capabilities, Collection<SessionFactory>> factoryFactory) {

    int maxSessions = getMaxSessions();

    Map<WebDriverInfo, Collection<SessionFactory>> allDrivers =
      discoverDrivers(maxSessions, factoryFactory);

    ImmutableMultimap.Builder<Capabilities, SessionFactory> sessionFactories =
      ImmutableMultimap.builder();

    addDriverFactoriesFromConfig(sessionFactories);
    addSpecificDrivers(allDrivers, sessionFactories);
    addDetectedDrivers(allDrivers, sessionFactories);
    addDriverConfigs(factoryFactory, sessionFactories);

    return sessionFactories.build().asMap();
  }

  public int getMaxSessions() {
    return Math.min(
      config.getInt(NODE_SECTION, "max-concurrent-sessions")
        .orElse(Runtime.getRuntime().availableProcessors()),
      Runtime.getRuntime().availableProcessors());
  }

  private void addDriverFactoriesFromConfig(ImmutableMultimap.Builder<Capabilities,
    SessionFactory> sessionFactories) {
    config.getAll(NODE_SECTION, "driver-factories").ifPresent(allConfigs -> {
      if (allConfigs.size() % 2 != 0) {
        throw new ConfigException("Expected each driver class to be mapped to a config");
      }

      Map<String, String> configMap = IntStream.range(0, allConfigs.size()/2).boxed()
        .collect(Collectors.toMap(i -> allConfigs.get(2*i), i -> allConfigs.get(2*i + 1)));

      configMap.forEach((clazz, config) -> {
        Capabilities stereotype = JSON.toType(config, Capabilities.class);
        SessionFactory sessionFactory = createSessionFactory(clazz, stereotype);
        sessionFactories.put(stereotype, sessionFactory);
      });
    });
  }

  private SessionFactory createSessionFactory(String clazz, Capabilities stereotype) {
    LOG.fine(String.format("Creating %s as instance of %s", clazz, SessionFactory.class));

    try {
      // Use the context class loader since this is what the `--ext`
      // flag modifies.
      Class<?> ClassClazz =
        Class.forName(clazz, true, Thread.currentThread().getContextClassLoader());
      Method create = ClassClazz.getMethod("create", Config.class, Capabilities.class);

      if (!Modifier.isStatic(create.getModifiers())) {
        throw new IllegalArgumentException(String.format(
          "Class %s's `create(Config, Capabilities)` method must be static", clazz));
      }

      if (!SessionFactory.class.isAssignableFrom(create.getReturnType())) {
        throw new IllegalArgumentException(String.format(
          "Class %s's `create(Config, Capabilities)` method must be static", clazz));
      }

      return (SessionFactory) create.invoke(null, config, stereotype);
    } catch (NoSuchMethodException e) {
      throw new IllegalArgumentException(String.format(
        "Class %s must have a static `create(Config, Capabilities)` method", clazz));
    } catch (ReflectiveOperationException e) {
      throw new IllegalArgumentException("Unable to find class: " + clazz, e);
    }
  }

  private void addDriverConfigs(
    Function<Capabilities, Collection<SessionFactory>> factoryFactory,
    ImmutableMultimap.Builder<Capabilities, SessionFactory> sessionFactories) {
    Multimap<WebDriverInfo, SessionFactory> driverConfigs = HashMultimap.create();
    int configElements = 3;
    config.getAll(NODE_SECTION, "driver-configuration").ifPresent(drivers -> {
      if (drivers.size() % configElements != 0) {
        throw new ConfigException("Expected each driver config to have three elements " +
                                  "(name, stereotype and max-sessions)");
      }

      drivers.stream()
        .filter(driver -> !driver.contains("="))
        .peek(driver -> LOG.warning(driver + " does not have the required 'key=value' " +
                                    "structure for the configuration"))
        .findFirst()
        .ifPresent(ignore -> {
          throw new ConfigException("One or more driver configs does not have the " +
                                    "required 'key=value' structure");
        });

      List<Map<String, String>> driversMap = new ArrayList<>();
      IntStream.range(0, drivers.size()/configElements).boxed()
        .forEach(i -> {
          ImmutableMap<String, String> configMap = ImmutableMap.of(
            drivers.get(i*configElements).split("=")[0],
            drivers.get(i*configElements).split("=")[1],
            drivers.get(i*configElements+1).split("=")[0],
            drivers.get(i*configElements+1).split("=")[1],
            drivers.get(i*configElements+2).split("=")[0],
            drivers.get(i*configElements+2).split("=")[1]
          );
          driversMap.add(configMap);
        });

      List<DriverService.Builder<?, ?>> builders = new ArrayList<>();
      ServiceLoader.load(DriverService.Builder.class).forEach(builders::add);

      List<WebDriverInfo> infos = new ArrayList<>();
      ServiceLoader.load(WebDriverInfo.class).forEach(infos::add);

      driversMap.forEach(configMap -> {
        if (!configMap.containsKey("stereotype")) {
          throw new ConfigException("Driver config is missing stereotype value. " + configMap);
        }
        Capabilities stereotype = JSON.toType(configMap.get("stereotype"), Capabilities.class);
        String configName = configMap.getOrDefault("name", "Custom Slot Config");
        int driverMaxSessions = Integer.parseInt(configMap.getOrDefault("max-sessions", "1"));
        Require.positive("Driver max sessions", driverMaxSessions);

        WebDriverInfo info = infos.stream()
          .filter(webDriverInfo -> webDriverInfo.isSupporting(stereotype))
          .findFirst()
          .orElseThrow(() ->
                         new ConfigException("Unable to find matching driver for %s", stereotype));

        WebDriverInfo driverInfoConfig = createConfiguredDriverInfo(info, stereotype, configName);

        builders.stream()
          .filter(builder -> builder.score(stereotype) > 0)
          .forEach(builder -> {
            int maxSessions = Math.min(info.getMaximumSimultaneousSessions(), driverMaxSessions);
            for (int i = 0; i < maxSessions; i++) {
              driverConfigs.putAll(driverInfoConfig, factoryFactory.apply(stereotype));
            }
          });
      });
    });
    driverConfigs.asMap().entrySet()
      .stream()
      .peek(this::report)
      .forEach(
        entry ->
          sessionFactories.putAll(entry.getKey().getCanonicalCapabilities(), entry.getValue()));
  }

  private void addDetectedDrivers(
    Map<WebDriverInfo, Collection<SessionFactory>> allDrivers,
    ImmutableMultimap.Builder<Capabilities, SessionFactory> sessionFactories) {
    if (!config.getBool(NODE_SECTION, "detect-drivers").orElse(true)) {
      return;
    }

    // Only specified drivers should be added, not all the detected ones
    if (config.getAll(NODE_SECTION, "drivers").isPresent()) {
      return;
    }

    allDrivers.entrySet()
      .stream()
      .peek(this::report)
      .forEach(
        entry ->
          sessionFactories.putAll(entry.getKey().getCanonicalCapabilities(), entry.getValue()));
  }

  private void addSpecificDrivers(
    Map<WebDriverInfo, Collection<SessionFactory>> allDrivers,
    ImmutableMultimap.Builder<Capabilities, SessionFactory> sessionFactories) {
    if (!config.getBool(NODE_SECTION, "detect-drivers").orElse(true) &&
        config.getAll(NODE_SECTION, "drivers").isPresent()) {
      String logMessage = "Specific drivers cannot be added if 'detect-drivers' is set to false";
      LOG.warning(logMessage);
      throw new ConfigException(logMessage);
    }

    List<String> drivers = config.getAll(NODE_SECTION, "drivers").orElse(new ArrayList<>())
      .stream()
      .map(String::toLowerCase)
      .collect(Collectors.toList());

    allDrivers.entrySet().stream()
      .filter(entry -> drivers.contains(entry.getKey().getDisplayName().toLowerCase()))
      .sorted(Comparator.comparing(entry -> entry.getKey().getDisplayName().toLowerCase()))
      .peek(this::report)
      .forEach(
        entry ->
          sessionFactories.putAll(entry.getKey().getCanonicalCapabilities(), entry.getValue()));
  }

  private Map<WebDriverInfo, Collection<SessionFactory>> discoverDrivers(
    int maxSessions, Function<Capabilities, Collection<SessionFactory>> factoryFactory) {

    if (!config.getBool(NODE_SECTION, "detect-drivers").orElse(true)) {
      return ImmutableMap.of();
    }

    // We don't expect duplicates, but they're fine
    List<WebDriverInfo> infos =
      StreamSupport.stream(ServiceLoader.load(WebDriverInfo.class).spliterator(), false)
        .filter(WebDriverInfo::isAvailable)
        .sorted(Comparator.comparing(info -> info.getDisplayName().toLowerCase()))
        .collect(Collectors.toList());

    // Same
    List<DriverService.Builder<?, ?>> builders = new ArrayList<>();
    ServiceLoader.load(DriverService.Builder.class).forEach(builders::add);

    Multimap<WebDriverInfo, SessionFactory> toReturn = HashMultimap.create();
    infos.forEach(info -> {
      Capabilities caps = info.getCanonicalCapabilities();
      builders.stream()
        .filter(builder -> builder.score(caps) > 0)
        .forEach(builder -> {
          for (int i = 0; i < Math.min(info.getMaximumSimultaneousSessions(), maxSessions); i++) {
            toReturn.putAll(info, factoryFactory.apply(caps));
          }
        });
    });

    return toReturn.asMap();
  }

  private WebDriverInfo createConfiguredDriverInfo(
    WebDriverInfo detectedDriver, Capabilities canonicalCapabilities, String displayName) {
    return new WebDriverInfo() {
      @Override
      public String getDisplayName() {
        return displayName;
      }

      @Override
      public Capabilities getCanonicalCapabilities() {
        return canonicalCapabilities;
      }

      @Override
      public boolean isSupporting(Capabilities capabilities) {
        return detectedDriver.isSupporting(capabilities);
      }

      @Override
      public boolean isSupportingCdp() {
        return detectedDriver.isSupportingCdp();
      }

      @Override
      public boolean isAvailable() {
        return detectedDriver.isAvailable();
      }

      @Override
      public int getMaximumSimultaneousSessions() {
        return detectedDriver.getMaximumSimultaneousSessions();
      }

      @Override
      public Optional<WebDriver> createDriver(Capabilities capabilities)
        throws SessionNotCreatedException {
        return Optional.empty();
      }
    };
  }

  private void report(Map.Entry<WebDriverInfo, Collection<SessionFactory>> entry) {
    StringBuilder caps = new StringBuilder();
    try (JsonOutput out = JSON.newOutput(caps)) {
      out.setPrettyPrint(false);
      out.write(entry.getKey().getCanonicalCapabilities());
    }

    LOG.info(String.format(
      "Adding %s for %s %d times",
      entry.getKey().getDisplayName(),
      caps.toString().replaceAll("\\s+", " "),
      entry.getValue().size()));
  }
}
