extends Node3D

@onready var _demo = $OverspaceTrophyRoom
@onready var _player = $TransportChamberPlayer
@onready var _legacy_camera: Camera3D = $OverspaceTrophyRoom/PlayerCamera
@onready var _gallery_portal: Node3D = $OverspaceTrophyRoom/Gallery/EarthOrbPortal
@onready var _earth_portal: Node3D = $OverspaceTrophyRoom/Worlds/EarthWorld/EarthArrivalPortal
@onready var _film_camera: Node = $GrinFilmCamera
@onready var _ray_renderer: Node = $RayBeamRenderer
@onready var _film_controller: Node = $FilmController
@onready var _summary_label: Label = $OverspaceTrophyRoom/CanvasLayer/DemoSummary
@onready var _overspace_debug_overlay: Control = $OverspaceTrophyRoom/CanvasLayer/OverspaceDebugOverlay
@onready var _gallery_field: Node = $GalleryFieldSource
@onready var _earth_field: Node = $EarthFieldSource
@onready var _locomotion_label: Label = $CanvasLayer/LocomotionStatusLabel

var _cooldown_remaining := 0.0
var _last_gallery_delta := 0.0
var _last_earth_delta := 0.0
var _advanced_telemetry_visible := false


func _ready() -> void:
	if _legacy_camera != null:
		_legacy_camera.current = false
		if _legacy_camera.has_method("SetInputEnabled"):
			_legacy_camera.call("SetInputEnabled", false, false)
	_configure_chamber_portals()

	var player_camera: Camera3D = _player.get_camera()
	if _demo.has_method("OverrideViewerCamera"):
		_demo.call("OverrideViewerCamera", player_camera)
	if _demo.has_method("SetExternalTraversalOwnerActive"):
		_demo.call("SetExternalTraversalOwnerActive", true)
	player_camera.current = true
	if _player.has_signal("loco_mode_changed"):
		_player.loco_mode_changed.connect(_on_locomotion_mode_changed)
	_on_locomotion_mode_changed(_player.GetLocomotionModeName())
	_set_advanced_telemetry_visible(false)
	_set_chamber_summary()
	_prime_portal_deltas()


func _exit_tree() -> void:
	if _film_controller != null:
		_film_controller.call("set_mode", 0)
	if _film_camera != null:
		_film_camera.set("UpdateEveryFrame", false)
		_film_camera.set_process(false)
	if _ray_renderer != null:
		_ray_renderer.set("UpdateEveryFrame", false)
		_ray_renderer.set_process(false)


func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventKey and event.pressed and not event.echo:
		if event.keycode == KEY_TAB:
			_set_advanced_telemetry_visible(not _advanced_telemetry_visible)
			get_viewport().set_input_as_handled()


func _process(delta: float) -> void:
	if _cooldown_remaining > 0.0:
		_cooldown_remaining = max(0.0, _cooldown_remaining - delta)
		_prime_portal_deltas()
		return

	var camera: Camera3D = _player.get_camera()
	_try_cross(_gallery_portal, camera.global_position, true)
	_try_cross(_earth_portal, camera.global_position, false)


func _try_cross(portal: Node3D, camera_position: Vector3, gallery_side: bool) -> void:
	if portal == null or not portal.has_method("SignedRadiusDelta"):
		return
	var current_delta := float(portal.call("SignedRadiusDelta", camera_position))
	var previous_delta := _last_gallery_delta if gallery_side else _last_earth_delta
	if gallery_side:
		_last_gallery_delta = current_delta
	else:
		_last_earth_delta = current_delta

	if previous_delta > 0.0 and current_delta <= 0.0 and portal.has_method("BuildExitTransform"):
		var mapped = portal.call("BuildExitTransform", _player.get_camera().global_transform)
		if mapped is Transform3D:
			_player.ApplyCameraTransform(mapped)
			if _film_controller != null and _film_controller.has_method("NotifyCameraTransformJump"):
				_film_controller.call("NotifyCameraTransformJump")
			_cooldown_remaining = 0.4
			_prime_portal_deltas()


func _prime_portal_deltas() -> void:
	var camera: Camera3D = _player.get_camera()
	if _gallery_portal != null and _gallery_portal.has_method("SignedRadiusDelta"):
		_last_gallery_delta = float(_gallery_portal.call("SignedRadiusDelta", camera.global_position))
	if _earth_portal != null and _earth_portal.has_method("SignedRadiusDelta"):
		_last_earth_delta = float(_earth_portal.call("SignedRadiusDelta", camera.global_position))


func _configure_chamber_portals() -> void:
	for portal in [_gallery_portal, _earth_portal]:
		if portal == null:
			continue
		portal.set("EnablePhaseLockedRemap", false)


func _set_chamber_summary() -> void:
	if _summary_label == null:
		return
	_summary_label.text = ""
	_summary_label.visible = false


func _set_advanced_telemetry_visible(visible: bool) -> void:
	_advanced_telemetry_visible = visible
	if _overspace_debug_overlay != null:
		_overspace_debug_overlay.visible = visible
	for field in [_gallery_field, _earth_field]:
		if field != null:
			field.set("DebugVizInGame", visible)


func _on_locomotion_mode_changed(mode_name: String) -> void:
	if _locomotion_label == null:
		return
	_locomotion_label.text = "Move: %s  |  WASD + mouse  |  Shift sprint  |  V walk/fly  |  G film  |  [ ] opacity  |  Tab telemetry  |  Esc Observatory" % mode_name
