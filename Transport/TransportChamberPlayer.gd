extends CharacterBody3D

signal loco_mode_changed(mode_name: String)

enum LocomotionMode { WALK, FLY }

const WALK_SPEED := 5.0
const SPRINT_MULTIPLIER := 2.0
const GRAVITY := 9.8
const MOUSE_SENSITIVITY := 0.0025
const PITCH_MIN := -1.4
const PITCH_MAX := 1.4

@onready var _head: Node3D = $Head
@onready var _camera: Camera3D = $Head/PlayerCamera

var _pitch := 0.0
var _input_enabled := true
var _loco_mode: LocomotionMode = LocomotionMode.WALK


func _ready() -> void:
	Input.mouse_mode = Input.MOUSE_MODE_CAPTURED


func _unhandled_input(event: InputEvent) -> void:
	if not _input_enabled:
		return
	if event is InputEventMouseMotion and Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		rotate_y(-event.relative.x * MOUSE_SENSITIVITY)
		_pitch = clamp(_pitch - event.relative.y * MOUSE_SENSITIVITY, PITCH_MIN, PITCH_MAX)
		_head.rotation.x = _pitch
	elif event is InputEventKey and event.pressed and not event.echo:
		if event.keycode == KEY_V:
			_toggle_locomotion_mode()
			get_viewport().set_input_as_handled()


func _physics_process(delta: float) -> void:
	if not _input_enabled:
		velocity = Vector3.ZERO
		return

	match _loco_mode:
		LocomotionMode.WALK:
			if not is_on_floor():
				velocity.y -= GRAVITY * delta
			elif velocity.y < 0.0:
				velocity.y = 0.0
		LocomotionMode.FLY:
			# Fly is fully view-relative: camera pitch controls climb/descent.
			# There are intentionally no dedicated rise/fall actions in this mode.
			velocity = Vector3.ZERO

	var input_dir := Vector3.ZERO
	if _loco_mode == LocomotionMode.FLY:
		var view_forward := -_camera.global_transform.basis.z
		var view_right := _camera.global_transform.basis.x
		if Input.is_action_pressed("move_forward"):
			input_dir += view_forward
		if Input.is_action_pressed("move_backward"):
			input_dir -= view_forward
		if Input.is_action_pressed("move_left"):
			input_dir -= view_right
		if Input.is_action_pressed("move_right"):
			input_dir += view_right
	else:
		if Input.is_action_pressed("move_forward"):
			input_dir -= basis.z
		if Input.is_action_pressed("move_backward"):
			input_dir += basis.z
		if Input.is_action_pressed("move_left"):
			input_dir -= basis.x
		if Input.is_action_pressed("move_right"):
			input_dir += basis.x

	var speed := WALK_SPEED
	if Input.is_action_pressed("move_sprint"):
		speed *= SPRINT_MULTIPLIER

	if input_dir.length_squared() > 0.0:
		input_dir = input_dir.normalized()
		velocity.x = input_dir.x * speed
		velocity.y = input_dir.y * speed if _loco_mode == LocomotionMode.FLY else velocity.y
		velocity.z = input_dir.z * speed
	else:
		if _loco_mode == LocomotionMode.FLY:
			velocity = velocity.move_toward(Vector3.ZERO, speed)
		else:
			velocity.x = move_toward(velocity.x, 0.0, speed)
			velocity.z = move_toward(velocity.z, 0.0, speed)

	move_and_slide()


func get_camera() -> Camera3D:
	return _camera


func SetInputEnabled(enabled: bool, release_mouse := true) -> void:
	_input_enabled = enabled
	if not enabled:
		velocity = Vector3.ZERO
		if release_mouse:
			Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
	elif Input.mouse_mode != Input.MOUSE_MODE_CAPTURED:
		Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
	var film_controller := get_parent().get_node_or_null("FilmController") if get_parent() != null else null
	if film_controller != null and film_controller.has_method("SetInputEnabled"):
		film_controller.call("SetInputEnabled", enabled)
	if get_parent() != null and get_parent().has_method("SetHudVisibleForGameplay"):
		get_parent().call("SetHudVisibleForGameplay", enabled)


func ApplyCameraTransform(camera_transform: Transform3D) -> void:
	# Portal traversal owns the body root. Velocity is intentionally zeroed for
	# this slice so a crossing cannot carry stale wall/floor contact impulses.
	var camera_euler := camera_transform.basis.get_euler()
	rotation = Vector3(0.0, camera_euler.y, 0.0)
	_pitch = clamp(camera_euler.x, PITCH_MIN, PITCH_MAX)
	_head.rotation.x = _pitch
	global_position = camera_transform.origin - global_basis * _head.position
	_camera.transform = Transform3D.IDENTITY
	velocity = Vector3.ZERO


func GetLocomotionModeName() -> String:
	return "Fly" if _loco_mode == LocomotionMode.FLY else "Walk"


func SetLocomotionModeForTesting(mode_name: String) -> void:
	var requested := mode_name.strip_edges().to_lower()
	var next_mode := LocomotionMode.FLY if requested == "fly" else LocomotionMode.WALK
	_set_locomotion_mode(next_mode)


func _toggle_locomotion_mode() -> void:
	var next_mode := LocomotionMode.FLY if _loco_mode == LocomotionMode.WALK else LocomotionMode.WALK
	_set_locomotion_mode(next_mode)


func _set_locomotion_mode(mode: LocomotionMode) -> void:
	if _loco_mode == mode:
		return
	_loco_mode = mode
	velocity.y = 0.0
	loco_mode_changed.emit(GetLocomotionModeName())
