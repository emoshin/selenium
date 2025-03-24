#!/usr/bin/env bash

set -eufo pipefail
# We want to see what's going on
set -x

# Now run the tests. The engflow build uses pinned browsers
# so this should be fine
# shellcheck disable=SC2046
bazel test --config=rbe-ci --build_tests_only \
  --keep_going --flaky_test_attempts=2 \
  //... -- $(cat .skipped-tests | tr '\n' ' ')

# Build the packages we want to ship to users
bazel build --config=rbe-ci --build_tag_filters=release-artifact //...
