# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]
- Upcoming features will be listed here.

## [0.2.0] - 2026-10-05

### Added

- Scene view panel with the point and collider counts and **Place Points** / **Done** / **Generate Colliders** buttons, so the whole workflow happens in the Scene view.
- Placing preview: a ghost point under the cursor plus the segment from the last point and the closing segment back to the first.
- Translucent walls showing the area the colliders will cover, and green outlines of the generated colliders while the generator is selected.
- Finish placing with Esc, Enter, a right-click (right-drag still orbits the camera) or the **Done** button; Backspace removes the last placed point.
- Points snap to scene geometry under the cursor (hold Shift to stay level with the previous point).
- Click a point to select it (Delete removes it); click the green dot in the middle of a segment to insert a point.
- **Basic Setup** sample that works in any render pipeline.

### Changed

- **Breaking:** the component moved to a runtime assembly (`Oox.WorldEdgeColliderGenerator`) and namespace (`Oox.WorldEdgeColliderGenerator`), so scenes that use it no longer report a missing script in player builds. Generating and clearing colliders moved to `WorldEdgeBoxColliderBuilder` in the editor assembly. Existing scenes keep their references.
- Generating colliders replaces previously generated ones instead of adding duplicates.
- Points are small draggable dots instead of a full move gizmo each; the selected point gets a move gizmo for vertical adjustments.
- The object's own transform gizmo is hidden while placing points so it can't be grabbed by accident.
- Requires Unity 6 (6000.0) or newer.
- Licensed under MIT.

### Fixed

- **Generate Box Colliders** only created the first box in edit mode; all boxes are now generated at once.
- Generating and clearing box colliders are now undoable, and work when **Box Parent** is unassigned (the generator itself is used).

## [0.1.0] - 2026-07-15

### Changed

- Replaced the mesh-based vertex input with a `Points` list that can be placed and edited directly in the Scene view (drag handles, click-to-place with the new "Place Points" tool, or edit the list in the inspector), removing the dependency on an authored mesh.

## [0.0.1] - 2025-02-23

### Added
- Added the World Edge Collider Generator from previous projects.