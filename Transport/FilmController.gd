class_name FilmController
extends Node

enum FilmMode { OFF, SNAPSHOT, LIVE }

const OPACITY_STEP := 0.10
const OPACITY_MIN := 0.0
const OPACITY_MAX := 1.0
const OVERLAY_BASE_ALPHA := 0.35
const SNAPSHOT_DURATION_S := 0.6
const SNAPSHOT_MAX_RENDER_STEPS := 12
const SNAPSHOT_STALE_DISTANCE := 0.5
const SHADING_DEPTH := 0
const SHADING_NORMAL_RGB := 1
const SHADING_NDOTV := 2
const SHADING_TWO_SIDED_NDOTV := 3
const SHADING_SEQUENCE := [
	SHADING_NORMAL_RGB,
	SHADING_NDOTV,
	SHADING_TWO_SIDED_NDOTV,
	SHADING_DEPTH,
]

@export var film_camera_path: NodePath
@export var film_plate_path: NodePath
@export var film_view_path: NodePath
@export var film_overlay_path: NodePath
@export var status_label_path: NodePath
@export var boundary_label_path: NodePath
@export var player_path: NodePath

var _mode: FilmMode = FilmMode.OFF
var _quality_name := "Preview"
var _opacity := 0.30
var _snapshot_timer := 0.0
var _snapshot_end_msec := 0
var _snapshot_steps_remaining := 0
var _snapshot_origin := Vector3.ZERO
var _snapshot_forced_stale := false
var _input_enabled := true
var _render_requested := false
var _shading_mode := SHADING_NORMAL_RGB

@onready var _film_camera: Node = get_node_or_null(film_camera_path)
@onready var _film_plate: Control = get_node_or_null(film_plate_path)
@onready var _film_view: TextureRect = get_node_or_null(film_view_path)
@onready var _film_overlay: CanvasItem = get_node_or_null(film_overlay_path)
@onready var _status_label: Label = get_node_or_null(status_label_path)
@onready var _boundary_label: Label = get_node_or_null(boundary_label_path)
@onready var _player: CharacterBody3D = get_node_or_null(player_path)


func _ready() -> void:
	if _film_camera != null:
		_shading_mode = int(_film_camera.get("ShadingMode"))
	_apply_shading_mode(_shading_mode, false)
	set_mode(FilmMode.OFF)


func _unhandled_input(event: InputEvent) -> void:
	if not _input_enabled:
		return
	if event is InputEventKey and event.pressed and not event.echo:
		match event.keycode:
			KEY_G:
				cycle_mode()
				get_viewport().set_input_as_handled()
			KEY_BRACKETLEFT:
				set_opacity(_opacity - OPACITY_STEP)
				get_viewport().set_input_as_handled()
			KEY_BRACKETRIGHT:
				set_opacity(_opacity + OPACITY_STEP)
				get_viewport().set_input_as_handled()
			KEY_N:
				cycle_shading_mode()
				get_viewport().set_input_as_handled()


func _process(delta: float) -> void:
	if _mode == FilmMode.SNAPSHOT:
		_update_snapshot_timer(delta)
		_update_status()


func _physics_process(delta: float) -> void:
	if _mode == FilmMode.SNAPSHOT:
		_update_snapshot_timer(delta)
	_run_render_step_if_requested()
	if _mode == FilmMode.SNAPSHOT:
		_update_status()


func _run_render_step_if_requested() -> void:
	if not _render_requested or _film_camera == null:
		return
	if _film_camera.has_method("RenderStep"):
		_film_camera.call("RenderStep")
		if _mode == FilmMode.SNAPSHOT and _snapshot_steps_remaining > 0:
			_snapshot_steps_remaining -= 1
			if _snapshot_steps_remaining <= 0:
				_freeze_snapshot()
	if _mode == FilmMode.SNAPSHOT:
		_update_snapshot_timer(0.0)
		_update_status()


func cycle_mode() -> void:
	var next_mode := FilmMode.SNAPSHOT
	if _mode == FilmMode.SNAPSHOT:
		next_mode = FilmMode.LIVE
	elif _mode == FilmMode.LIVE:
		next_mode = FilmMode.OFF
	set_mode(next_mode)


func cycle_shading_mode() -> void:
	var index := SHADING_SEQUENCE.find(_shading_mode)
	if index < 0:
		index = 0
	else:
		index = (index + 1) % SHADING_SEQUENCE.size()
	_apply_shading_mode(SHADING_SEQUENCE[index], true)


func set_mode(mode: FilmMode) -> void:
	_mode = mode
	match _mode:
		FilmMode.OFF:
			_snapshot_timer = 0.0
			_snapshot_end_msec = 0
			_snapshot_steps_remaining = 0
			_snapshot_forced_stale = false
			_set_film_compute(false)
			_set_film_visible(false)
		FilmMode.SNAPSHOT:
			_apply_quality_interactive()
			_snapshot_timer = SNAPSHOT_DURATION_S
			_snapshot_end_msec = 0
			_snapshot_steps_remaining = SNAPSHOT_MAX_RENDER_STEPS
			_snapshot_origin = _player.global_position if _player != null else Vector3.ZERO
			_snapshot_forced_stale = false
			_restart_film_pass()
			_set_film_visible(true)
			_set_film_compute(true)
		FilmMode.LIVE:
			_apply_quality_preview()
			_snapshot_timer = 0.0
			_snapshot_end_msec = 0
			_snapshot_steps_remaining = 0
			_snapshot_forced_stale = false
			_restart_film_pass()
			_set_film_visible(true)
			_set_film_compute(true)
	set_opacity(_opacity)
	_update_status()


func set_opacity(value: float) -> void:
	_opacity = clamp(value, OPACITY_MIN, OPACITY_MAX)
	if _film_camera != null:
		if _film_camera.has_method("SetFilmOpacityForTesting"):
			_film_camera.call("SetFilmOpacityForTesting", _opacity)
		else:
			_film_camera.set("FilmOpacity", _opacity)
	if _film_view != null:
		_film_view.modulate.a = _opacity
	if _film_overlay != null:
		_film_overlay.modulate.a = OVERLAY_BASE_ALPHA * _opacity
	_update_status()


func SetInputEnabled(enabled: bool) -> void:
	_input_enabled = enabled
	if not enabled and _mode == FilmMode.LIVE:
		_set_film_compute(false)
	elif enabled and _mode == FilmMode.LIVE:
		_set_film_compute(true)
	_update_status()


func GetModeName() -> String:
	match _mode:
		FilmMode.OFF:
			return "OFF"
		FilmMode.SNAPSHOT:
			return "SNAPSHOT"
		FilmMode.LIVE:
			return "LIVE"
	return "UNKNOWN"


func GetOpacityPercent() -> int:
	return int(round(_opacity * 100.0))


func GetQualityName() -> String:
	return _quality_name


func GetShadingModeName() -> String:
	return _get_shading_mode_label(_shading_mode)


func IsComputeActive() -> bool:
	if _mode == FilmMode.SNAPSHOT:
		_update_snapshot_timer(0.0)
	return _render_requested


func NotifyCameraTransformJump() -> void:
	match _mode:
		FilmMode.LIVE:
			_restart_film_pass()
			_set_film_compute(true)
		FilmMode.SNAPSHOT:
			_snapshot_forced_stale = true
	_update_status()


func NotifyFieldStrengthChanged() -> void:
	match _mode:
		FilmMode.LIVE:
			_restart_film_pass()
			_set_film_compute(true)
		FilmMode.SNAPSHOT:
			_snapshot_forced_stale = true
	_update_status()


func _apply_quality_preview() -> void:
	_quality_name = "Preview 80x45"
	_apply_quality(0.5, 16, 16, 80.0)


func _apply_quality_interactive() -> void:
	_quality_name = "Interactive 160x90"
	_apply_quality(1.0, 8, 8, 80.0)


func _apply_quality(scale: float, rows_cap: int, rows_per_step: int, budget_ms: float) -> void:
	if _film_camera == null:
		return
	_film_camera.set("FilmResolutionScale", scale)
	_film_camera.set("MaxRowsPerFrameCap", rows_cap)
	_film_camera.set("UpdateEveryFrameMaxRowsPerStep", rows_per_step)
	_film_camera.set("UpdateEveryFrameBudgetMs", budget_ms)
	_film_camera.set("UseThreadedBands", false)
	_film_camera.set("ThreadedBandWorkerCount", 1)
	_film_camera.set("ThreadedBandRowsPerChunk", rows_per_step)


func _restart_film_pass() -> void:
	if _film_camera != null and _film_camera.has_method("ResetFilmPassManual"):
		_film_camera.call("ResetFilmPassManual")


func _set_film_compute(enabled: bool) -> void:
	_render_requested = enabled
	if _film_camera != null:
		_film_camera.set("UpdateEveryFrame", false)


func _apply_shading_mode(shading_mode: int, reset_pass: bool) -> void:
	_shading_mode = shading_mode
	if _film_camera != null:
		_film_camera.set("ShadingMode", shading_mode)
	if reset_pass:
		if _mode == FilmMode.LIVE:
			_restart_film_pass()
			_set_film_compute(true)
		elif _mode == FilmMode.SNAPSHOT:
			set_mode(FilmMode.SNAPSHOT)
			return
	_update_status()


func _update_snapshot_timer(delta: float) -> void:
	if _snapshot_timer > 0.0:
		_snapshot_timer = max(0.0, _snapshot_timer - delta)


func _freeze_snapshot() -> void:
	_snapshot_timer = 0.0
	_snapshot_end_msec = 0
	_snapshot_steps_remaining = 0
	_set_film_compute(false)


func _set_film_visible(visible: bool) -> void:
	if _film_plate != null:
		_film_plate.visible = visible
	if _film_view != null:
		_film_view.visible = visible
	if _film_overlay != null:
		_film_overlay.visible = visible
	if _boundary_label != null:
		_boundary_label.visible = visible


func _update_status() -> void:
	if _status_label == null:
		return
	var mode_name := GetModeName()
	var state := ""
	if _mode == FilmMode.SNAPSHOT and _player != null:
		if _snapshot_forced_stale or _player.global_position.distance_to(_snapshot_origin) > SNAPSHOT_STALE_DISTANCE:
			state = " stale"
		elif _render_requested:
			state = " rendering"
		else:
			state = " frozen"
	var opacity_text := "Hidden" if _mode == FilmMode.OFF else str(GetOpacityPercent()) + "%"
	_status_label.text = "Film: %s%s | %s | Opacity: %s | Shading: %s" % [
		mode_name,
		state,
		_quality_name,
		opacity_text,
		GetShadingModeName(),
	]


func _get_shading_mode_label(shading_mode: int) -> String:
	match shading_mode:
		SHADING_NORMAL_RGB:
			return "Geometric Normal Debug"
		SHADING_NDOTV:
			return "NdotV"
		SHADING_TWO_SIDED_NDOTV:
			return "Two-sided NdotV"
		SHADING_DEPTH:
			return "Depth"
	return "Unknown"
