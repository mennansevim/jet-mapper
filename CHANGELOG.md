# Changelog

All notable changes to JetMapper will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed
- Fixed memory usage claims from 500%+ to up to 80% (more accurate)

### Added
- LICENSE file (MIT)
- .editorconfig for code style consistency
- CONTRIBUTING.md for community guidelines
- CHANGELOG.md for tracking changes

### Fixed
- .gitignore updated to properly handle nuget-packages folder
- Public assets path configuration improved

## [1.2.4] - 2025-01-XX

### Fixed
- Documentation improvements

## [1.2.3] - 2025-01-XX

### Changed
- Optimized README formatting for all platforms
- Enhanced visual presentation

## [1.2.2] - 2025-01-XX

### Fixed
- Converted README from HTML to Markdown for better NuGet.org rendering
- Improved documentation display across all platforms

## [1.2.1] - 2025-01-XX

### Fixed
- Fixed README image display on NuGet and GitHub
- Updated image paths to use GitHub raw URLs

## [1.2.0] - 2025-01-XX

### Changed
- Fixed compiler warnings for cleaner builds
- Enhanced documentation with social links
- Code quality improvements

## [1.1.0] - 2025-01-XX

### Added
- Extended platform support: .NET Standard 2.0/2.1
- .NET Framework 4.6.2/4.7.2/4.8 support
- .NET 6/7/8/9 support
- Cross-platform compatibility for maximum reach

## [1.0.0] - 2025-01-XX

### Added
- Initial release of JetMapper
- **Performance**: 2-4x faster than AutoMapper
- **Memory Efficiency**: Up to 80% memory savings in complex scenarios
- **Fluent API**: Builder Pattern with Set, SetIf, SetFirstIfExist, Ignore
- **Lifecycle Hooks**: BeforeMap and AfterMap support
- **Async Mapping**: With progress tracking for large datasets
- **Diff Mapping**: Object comparison and change detection
- **Snapshot & Restore**: Undo/redo functionality
- **Mapping Validator**: Compile-time validation
- **Type Converters**: Custom type conversion support
- **Diagnostic Tools**: Performance profiling capabilities
- **Partial Merge**: Selective property merging
- Comprehensive documentation (English & Turkish)
- Zero configuration required
- Production ready

### Features
- Simple one-line mapping with `FastMapTo<T>()`
- Complex mappings with `Builder()` pattern
- Collection mapping support
- Custom property transformations
- Conditional property assignments
- Priority-based property selection
- Enhanced type compatibility checks
- Minimal memory allocation
- Expression tree compilation for maximum performance

[Unreleased]: https://github.com/mennansevim/jet-mapper/compare/v1.2.4...HEAD
[1.2.4]: https://github.com/mennansevim/jet-mapper/compare/v1.2.3...v1.2.4
[1.2.3]: https://github.com/mennansevim/jet-mapper/compare/v1.2.2...v1.2.3
[1.2.2]: https://github.com/mennansevim/jet-mapper/compare/v1.2.1...v1.2.2
[1.2.1]: https://github.com/mennansevim/jet-mapper/compare/v1.2.0...v1.2.1
[1.2.0]: https://github.com/mennansevim/jet-mapper/compare/v1.1.0...v1.2.0
[1.1.0]: https://github.com/mennansevim/jet-mapper/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/mennansevim/jet-mapper/releases/tag/v1.0.0

