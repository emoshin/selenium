CDP_VERSIONS = [
    "v85",  # Required by Firefox
    "v94",
    "v95",
    "v96",
]

CDP_DEPS = ["//java/src/org/openqa/selenium/devtools/%s" % v for v in CDP_VERSIONS]
