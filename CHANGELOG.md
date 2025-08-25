# Changelog

<!---
## [x.y.z] - yyyy-mm-dd
### Added
### Changed
### Deprecated
### Removed
### Fixed
### Security
--->

## [Unreleased]
### Added
- Initial codebase based on the `frog.collections`
[package](https://github.com/dngulin/frogalicious-project/tree/main/Frogalicious/Packages/frog.collections).
- Initial `CHANGELOG.md` file
- `TempRefLsit` and `NativeRefList` types
- `DeallocApi` pattern to generate hierarchical dealloc methods for structs containing `NativeRefList`
- `AsSpan` and `AsreadonlySpan` extensions
- `README.md` file

### Changed
- All `RefList` variants provide unified internal extension methods
- Public API extension methods are provided by source generators
