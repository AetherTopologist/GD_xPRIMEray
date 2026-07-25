# Spawn Pose Architecture (Future Cleanup)

This note records follow-up design work for Transport Chamber spawn ownership. It is not part of the narrow Hello Observatory 1.4 spawn/collision repair.

## Marker3D Spawn Definitions

Future chamber presets should define authored spawn poses with explicit `Marker3D` nodes in the scene tree. Each preset can then own a named camera-space spawn marker, such as `GallerySpawnCamera` or `HermeticSpawnCamera`, instead of storing spawn vectors inside controller scripts.

## PlayerRoot vs Camera Transform Ownership

The player root owns collision, gravity, and CharacterBody movement. The camera owns the observer eye pose. APIs that accept a camera-space transform should say so in their name and should convert to player-root space by subtracting the camera local offset. APIs that accept a player-root transform should remain separate.

## Preset Initialization vs Teleport

Preset activation should prepare visibility, collision, field sources, and boundary volumes before any player pose is accepted. Teleporting the player should then be a separate step that can validate the target spawn, clear velocity, and settle safely without depending on hidden preset side effects.

## Lesson-Specific Observation Markers

Future lessons may need observation markers that are not player spawns. These should be named separately from traversal and spawn markers so tutorials can point the camera, film, or diagnostics toward a lesson target without changing player collision ownership.
