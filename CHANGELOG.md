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
- README and LICENSE files
- Move semantics extension
- `[RefList]` types with different allocators:
  - `RefList` - uses persistent allocator
  - `TempRefList` - uses temporary allocator
  - `ManagedRefList` - uses managed allocations & GC
- Code generators to produce `[RefList]` public API
- Trait attribute system:
  - `[ExplicitCopy]` - basic trait to forbid implicit copies
  - `[DynSized]` - trait to indicate dynamically sized collections
  - `[Dealloc]`, `[TempAlloc]` - (de)allocation behaviors trait
- Trait-related code generators and analyzers
- `[RefPath]` referencing rules
- Memory safety analyzer (borrow checker)
- Unit tests for Roslyn analyzers and code generators
- ECS-like data processing:
  - Data layout attributes: `[KsiComponent]`, `[KsiEntity]`, `[KsiArchetype]`, `[KsiDomain]`
  - Data querying attributes: `[KsiQuery]`, `[KsiQueryParam]`