# Glowing Heart Godot Fixture Export (Preview)

Generated: 2026-06-22T00:55:48Z

Runtime executed: false

Parity claim: NONE

Source: static_tscn_text_scan

## Fixture

| Field | Value |
|---|---|
| Name | fixture_hermetic_observatory_grin |
| Path | Fixtures/fixture_hermetic_observatory_grin.tscn |
| File Size | 5078 |
| Lines | 136 |

## Scene Header

```txt
[gd_scene load_steps=12 format=3]
```

## Classified Hints

| Hint | Value |
|---|---|
| hasCamera | true |
| hasFieldSource | true |
| hasGrinSignal | true |
| hasHermeticSignal | true |
| hasObservatorySignal | true |
| hasReceiverSignal | true |
| hasClosureSignal | true |
| hasBoundarySignal | false |
| hasWormholeSignal | false |

## External Resources

| Type | Path | Id |
|---|---|---|
| Script | res://RayBeamRenderer.cs | 1_rbr |
| Script | res://FieldSource3D.cs | 2_field |

## Nodes

| Name | Type | Parent |
|---|---|---|
| FixtureHermeticObservatoryGrin | Node3D |  |
| Camera3D | Camera3D | . |
| receiver_front | StaticBody3D | . |
| MeshInstance3D | MeshInstance3D | receiver_front |
| CollisionShape3D | CollisionShape3D | receiver_front |
| receiver_back | StaticBody3D | . |
| MeshInstance3D | MeshInstance3D | receiver_back |
| CollisionShape3D | CollisionShape3D | receiver_back |
| receiver_left | StaticBody3D | . |
| MeshInstance3D | MeshInstance3D | receiver_left |
| CollisionShape3D | CollisionShape3D | receiver_left |
| receiver_right | StaticBody3D | . |
| MeshInstance3D | MeshInstance3D | receiver_right |
| CollisionShape3D | CollisionShape3D | receiver_right |
| receiver_floor | StaticBody3D | . |
| MeshInstance3D | MeshInstance3D | receiver_floor |
| CollisionShape3D | CollisionShape3D | receiver_floor |
| receiver_ceiling | StaticBody3D | . |
| MeshInstance3D | MeshInstance3D | receiver_ceiling |
| CollisionShape3D | CollisionShape3D | receiver_ceiling |
| FieldSource3D | Node3D | . |
| RayBeamRenderer | Node3D | . |

## Interesting References

### FieldSource3D

Count: 2

```txt
[ext_resource type="Script" path="res://FieldSource3D.cs" id="2_field"]
[node name="FieldSource3D" type="Node3D" parent="."]
```

### Camera3D

Count: 2

```txt
[node name="Camera3D" type="Camera3D" parent="."]
CameraPath = NodePath("../Camera3D")
```

### Grin

Count: 3

```txt
; Fixture: hermetic observatory GRIN
; Purpose: sealed calibration chamber for full-pixel validation with GRIN field enabled.
[node name="FixtureHermeticObservatoryGrin" type="Node3D"]
```

### GRIN

Count: 3

```txt
; Fixture: hermetic observatory GRIN
; Purpose: sealed calibration chamber for full-pixel validation with GRIN field enabled.
[node name="FixtureHermeticObservatoryGrin" type="Node3D"]
```

### Hermetic

Count: 8

```txt
; Fixture: hermetic observatory GRIN
[node name="FixtureHermeticObservatoryGrin" type="Node3D"]
[node name="receiver_front" type="StaticBody3D" parent="." groups=["fixture_background", "fixture_geometry", "hermetic_receiver", "raytrace_geometry"]]
[node name="receiver_back" type="StaticBody3D" parent="." groups=["fixture_background", "fixture_geometry", "hermetic_receiver", "raytrace_geometry"]]
[node name="receiver_left" type="StaticBody3D" parent="." groups=["fixture_background", "fixture_geometry", "hermetic_receiver", "raytrace_geometry"]]
```

### Observatory

Count: 2

```txt
; Fixture: hermetic observatory GRIN
[node name="FixtureHermeticObservatoryGrin" type="Node3D"]
```

### Ray

Count: 11

```txt
; Contract: missHits == 0; curved transport rays must still terminate on a receiver wall.
[ext_resource type="Script" path="res://RayBeamRenderer.cs" id="1_rbr"]
[node name="receiver_front" type="StaticBody3D" parent="." groups=["fixture_background", "fixture_geometry", "hermetic_receiver", "raytrace_geometry"]]
[node name="receiver_back" type="StaticBody3D" parent="." groups=["fixture_background", "fixture_geometry", "hermetic_receiver", "raytrace_geometry"]]
[node name="receiver_left" type="StaticBody3D" parent="." groups=["fixture_background", "fixture_geometry", "hermetic_receiver", "raytrace_geometry"]]
```

### Receiver

Count: 19

```txt
; Contract: missHits == 0; curved transport rays must still terminate on a receiver wall.
[node name="receiver_front" type="StaticBody3D" parent="." groups=["fixture_background", "fixture_geometry", "hermetic_receiver", "raytrace_geometry"]]
[node name="MeshInstance3D" type="MeshInstance3D" parent="receiver_front"]
[node name="CollisionShape3D" type="CollisionShape3D" parent="receiver_front"]
[node name="receiver_back" type="StaticBody3D" parent="." groups=["fixture_background", "fixture_geometry", "hermetic_receiver", "raytrace_geometry"]]
```

### Transport

Count: 1

```txt
; Contract: missHits == 0; curved transport rays must still terminate on a receiver wall.
```

### Renderer

Count: 2

```txt
[ext_resource type="Script" path="res://RayBeamRenderer.cs" id="1_rbr"]
[node name="RayBeamRenderer" type="Node3D" parent="."]
```

## Limitations

- Static text scan only.
- Godot runtime was not executed.
- Scene graph was not instantiated.
- Exported values are metadata hints, not validated runtime state.
- No parity or closure claim is made.
