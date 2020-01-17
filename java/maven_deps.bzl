load("@rules_jvm_external//:defs.bzl", "maven_install")
load("@rules_jvm_external//:specs.bzl", "maven")

def selenium_java_deps():
    jetty_version = "9.4.25.v20191220"
    netty_version = "4.1.44.Final"

    maven_install(
        artifacts = [
            "com.beust:jcommander:1.78",
            "com.github.javaparser:javaparser-core:3.15.8",
            "com.google.code.gson:gson:2.8.6",
            "com.google.guava:guava:28.2-jre",
            "com.google.auto:auto-common:0.10",
            "com.google.auto.service:auto-service:1.0-rc6",
            "com.google.auto.service:auto-service-annotations:1.0-rc6",
            "com.squareup.okhttp3:okhttp:4.3.0",
            "com.typesafe.netty:netty-reactive-streams:2.0.4",
            "io.lettuce:lettuce-core:5.2.1.RELEASE",
            "io.netty:netty-buffer:%s" % netty_version,
            "io.netty:netty-codec-haproxy:%s" % netty_version,
            "io.netty:netty-codec-http:%s" % netty_version,
            "io.netty:netty-common:%s" % netty_version,
            "io.netty:netty-handler:%s" % netty_version,
            "io.netty:netty-transport:%s" % netty_version,
            "io.opentracing:opentracing-api:0.33.0",
            "io.opentracing:opentracing-noop:0.33.0",
            "io.opentracing.contrib:opentracing-tracerresolver:0.1.8",
            "it.ozimov:embedded-redis:0.7.2",
            "javax.servlet:javax.servlet-api:3.1.0",
            maven.artifact(
                group = "junit",
                artifact = "junit",
                version = "4.13",
                exclusions = [
                    "org.hamcrest:hamcrest-all",
                    "org.hamcrest:hamcrest-core",
                    "org.hamcrest:hamcrest-library",
                ],
            ),
            "net.bytebuddy:byte-buddy:1.10.6",
            "net.sourceforge.htmlunit:htmlunit:2.36.0",
            "net.sourceforge.htmlunit:htmlunit-core-js:2.36.0",
            "org.apache.commons:commons-exec:1.3",
            "org.assertj:assertj-core:3.14.0",
            "org.asynchttpclient:async-http-client:2.10.4",
            "org.eclipse.jetty:jetty-http:%s" % jetty_version,
            "org.eclipse.jetty:jetty-security:%s" % jetty_version,
            "org.eclipse.jetty:jetty-server:%s" % jetty_version,
            "org.eclipse.jetty:jetty-servlet:%s" % jetty_version,
            "org.eclipse.jetty:jetty-servlets:%s" % jetty_version,
            "org.eclipse.jetty:jetty-util:%s" % jetty_version,
            "org.eclipse.mylyn.github:org.eclipse.egit.github.core:2.1.5",
            "org.hamcrest:hamcrest:2.2",
            "org.mockito:mockito-core:3.2.4",
            "org.slf4j:slf4j-jdk14:1.7.30",
            "org.testng:testng:6.14.3",
            "org.zeromq:jeromq:0.5.1",
            "xyz.rogfam:littleproxy:2.0.0-beta-5",
            maven.artifact(
                group = "org.seleniumhq.selenium",
                artifact = "htmlunit-driver",
                version = "2.36.0",
                exclusions = [
                    "org.seleniumhq.selenium:selenium-api",
                    "org.seleniumhq.selenium:selenium-remote-driver",
                    "org.seleniumhq.selenium:selenium-support",
                ],
            ),
        ],
        excluded_artifacts = [
            "org.hamcrest:hamcrest-all", # Replaced by hamcrest 2
            "org.hamcrest:hamcrest-core",
            "io.netty:netty-all", # Depend on the actual things you need
        ],
        fail_on_missing_checksum = True,
        fetch_sources = True,
        strict_visibility = True,
        repositories = [
            "https://repo1.maven.org/maven2",
            "https://jcenter.bintray.com/",
            "https://maven.google.com",
        ],
        maven_install_json = "@selenium//java:maven_install.json",
    )
