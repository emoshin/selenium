CDP_VERSIONS = [
    "v85",  # Required by Firefox
    "v101",
    "v102",
    "v103",
]

CDP_DEPS = ["//java/src/org/openqa/selenium/devtools/%s" % v for v in CDP_VERSIONS]
