# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.3.0]

### Added
- Language switcher in the Settings panel: English and Simplified Chinese (简体中文).
  English is the default; the choice is remembered afterwards.
- MIT license, English README and this changelog.

### Changed
- Source code comments translated to English.

## [0.2.2]

### Fixed
- "Use an existing keyFiles folder…" no longer copies the whole working folder into itself when the
  working folder (or one of its parents) is selected. It now refuses, copies only the selected
  folder's top level, and asks for confirmation first.

### Changed
- The default working folder is now `Documents\reXIP-Gui`.
- The last used paths are remembered between runs.

## [0.2.1]

### Added
- The window title and the log show the version number.

### Changed
- Process auto-selection prefers the game itself (`DJMax`) over the launcher.

## [0.2.0]

### Added
- "Use an existing keyFiles folder…" for users who already have dumped keys.

## [0.1.0]

- Initial release.
