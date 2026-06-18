# Upgrade Guide

## 0.1.0 to 1.0.0

No breaking runtime data migrations are required. The package version was promoted to `1.0.0` for release preparation, package metadata now exposes documentation/changelog/license links, and late-phase economy/crafting APIs are explicitly marked experimental.

Recommended checks after upgrading:

1. Run the dashboard package validation.
2. Run edit mode and play mode tests.
3. Review any compile warnings related to experimental APIs before shipping production code that depends on them.
