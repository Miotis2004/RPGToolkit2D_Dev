# Changelog

All notable changes to RPG Toolkit 2D will be documented in this file.

## [0.1.0] - 2026-06-17

### Added
- Recorded Phase 0 repository and package audit decisions.
- Confirmed the Unity development host uses Unity `6000.5.0f1`, which is Unity 6-compatible.
- Established `com.sixstringsyn.rpgtoolkit2d` as the Unity Package Manager identifier.
- Established `SixStringSyn.RPGToolkit2D` as the root namespace for runtime, editor, and test code.
- Selected the MIT License as the initial package license.
- Recorded package author metadata as `Six String Syn`.
- Confirmed the package will be developed in `Packages/com.sixstringsyn.rpgtoolkit2d` and installed through Unity Package Manager as an embedded/local package during development.

### Known Gaps
- `com.unity.cinemachine` and `com.unity.addressables` are required by the development plan but are not currently present in `Packages/manifest.json`.
- Unity Editor validation, package resolution, edit mode tests, and play mode tests still need to be run in an environment with Unity available.
