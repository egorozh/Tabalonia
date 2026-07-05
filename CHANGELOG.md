# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [12.0.0]

### Added
- Tab detach and reattach support (#29)
- Chrome-like drag sessions: a dragged tab now attaches to another tab strip as soon as
  the pointer moves over it (no need to release the button), keeps following the pointer
  inside the target strip, and detaches again when dragged away vertically
- The target window is raised to the foreground when a tab attaches to it, so docking into
  a background window is visible
- Test project (`Tabalonia.Tests`) with headless Avalonia tests, including cross-window
  drag session tests
- CI: build/test on push and PR, NuGet publish on `v*` tags
- SourceLink and symbol packages (snupkg)

### Changed
- Library now multi-targets `net8.0` and `net10.0`
- A tab detached mid-drag now keeps following the pointer in one continuous gesture
  (previously the drag ended at the moment of detaching)

### Fixed
- Detached window did not inherit the source `TabsControl.Margin`, so the tab strip lost
  its top offset
- Tabs within `FixedHeaderCount` could be detached by dragging them out of the strip
- `KeyNotFoundException` in `TabsPanel` when a tab was re-detached after being dragged
  into another strip (the layout used a stale position cache); the panel now falls back
  to the tab's live position

## [0.12.0]

### Added
- Avalonia 12.0 support

## [0.10.x]

### Added
- Left and right content controls (#27)
- Tab close events (#18)
