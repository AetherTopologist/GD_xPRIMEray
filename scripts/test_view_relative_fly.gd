extends SceneTree

var root_scene: Node
var player: CharacterBody3D

func _initialize() -> void:
	root_scene = load("res://ObservatoryWorkbench.tscn").instantiate()
	get_root().add_child(root_scene)
	await _settle(8)
	player = root_scene.get_node("PlayableWorld/TransportChamberWorld/TransportChamberPlayer")
	player.SetInputEnabled(true, false)
	player.SetLocomotionModeForTesting("walk")
	await _settle(4)

	var walk_position := player.global_position
	_send_key(KEY_V)
	await _settle(2)
	_assert(player.GetLocomotionModeName() == "Fly", "V enters Fly")
	_assert(player.global_position.distance_to(walk_position) < 0.01, "V transition does not jump")

	var camera_head: Node3D = player.get_node("Head")
	# Positive camera pitch is upward in this scene's mouse-look convention.
	camera_head.rotation.x = deg_to_rad(30.0)
	var upward_origin := player.global_position
	Input.action_press("move_forward")
	await _settle(12)
	Input.action_release("move_forward")
	var upward_position := player.global_position
	print("upward origin=", upward_origin, " position=", upward_position, " camera_forward=", -player.get_camera().global_transform.basis.z)
	_assert(upward_position.y > upward_origin.y + 0.15, "upward view-relative W climbs")

	camera_head.rotation.x = deg_to_rad(-30.0)
	var downward_origin := player.global_position
	Input.action_press("move_forward")
	await _settle(12)
	Input.action_release("move_forward")
	var downward_position := player.global_position
	_assert(downward_position.y < downward_origin.y - 0.15, "downward view-relative W descends")

	camera_head.rotation.x = 0.0
	var strafe_origin := player.global_position
	Input.action_press("move_right")
	await _settle(12)
	Input.action_release("move_right")
	var strafe_delta := player.global_position - strafe_origin
	var camera_right: Vector3 = player.get_camera().global_transform.basis.x
	_assert(strafe_delta.dot(camera_right) > 0.15, "D strafes along camera right")

	var q_origin := player.global_position
	_send_key(KEY_Q)
	await _settle(4)
	_assert(player.global_position.distance_to(q_origin) < 0.02, "Q does not move player")

	print("VIEW RELATIVE FLY PASS upward_dy=%.3f downward_dy=%.3f strafe_camera_right=%.3f" % [upward_position.y - upward_origin.y, downward_position.y - downward_origin.y, strafe_delta.dot(camera_right)])
	quit(0)

func _send_key(code: Key) -> void:
	var event := InputEventKey.new()
	event.keycode = code
	event.physical_keycode = code
	event.pressed = true
	event.echo = false
	Input.parse_input_event(event)

func _settle(frames: int) -> void:
	for _i in range(frames):
		await process_frame

func _assert(condition: bool, message: String) -> void:
	if not condition:
		push_error("VIEW RELATIVE FLY FAIL: " + message)
		quit(1)
