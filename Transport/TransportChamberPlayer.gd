extends CharacterBody3D

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


func _ready() -> void:
	Input.mouse_mode = Input.MOUSE_MODE_CAPTURED


func _unhandled_input(event: InputEvent) -> void:
	if not _input_enabled:
		return
	if event is InputEventMouseMotion and Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		rotate_y(-event.relative.x * MOUSE_SENSITIVITY)
		_pitch = clamp(_pitch - event.relative.y * MOUSE_SENSITIVITY, PITCH_MIN, PITCH_MAX)
		_head.rotation.x = _pitch


func _physics_process(delta: float) -> void:
	if not _input_enabled:
		velocity = Vector3.ZERO
		return

	if not is_on_floor():
		velocity.y -= GRAVITY * delta
	elif velocity.y < 0.0:
		velocity.y = 0.0

	var input_dir := Vector3.ZERO
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
		velocity.z = input_dir.z * speed
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
