extends SceneTree

var scene_root: Node
var chamber: Node
var film: Node
var film_controller: Node
var field_dial: Node

func _initialize() -> void:
	scene_root = load("res://ObservatoryWorkbench.tscn").instantiate()
	get_root().add_child(scene_root)
	await process_frame
	await process_frame
	await process_frame
	chamber = scene_root.get_node("PlayableWorld/TransportChamberWorld")
	film = chamber.get_node("GrinFilmCamera")
	film_controller = chamber.get_node("FilmController")
	field_dial = chamber.get_node("FieldDialController")
	await _settle(30)

	_send_key(KEY_F)
	await _settle(4)
	_assert(field_dial.call("GetExperimentName") == "Gallery", "F preserves Gallery experiment")
	_assert(_all_field_sources_visible(), "F enables all field structures")

	_send_key(KEY_H)
	await _settle(8)
	_assert(field_dial.call("GetExperimentName") == "Gallery", "H does not change experiment")
	_assert(field_dial.call("GetPresentationName") == "Hermetic", "H selects Hermetic presentation")
	_assert(not bool(film.get("ProbeViewAvailable")), "Hermetic starts without sealed authority")

	_send_key(KEY_E)
	await _settle(8)
	_assert(field_dial.call("GetExperimentName") == "Hermetic", "E selects Hermetic experiment")
	_assert(field_dial.call("GetPresentationName") == "Gallery", "E resets presentation baseline")
	_assert(not bool(film.get("ProbeViewAvailable")), "E leaves no stale sealed authority")
	_send_key(KEY_H)
	await _settle(4)
	_assert(field_dial.call("GetExperimentName") == "Hermetic", "H preserves Hermetic experiment")
	_assert(field_dial.call("GetPresentationName") == "Hermetic", "H selects Hermetic presentation")

	_send_key(KEY_G)
	await _settle(2)
	_assert(film_controller.call("GetModeName") == "SNAPSHOT", "G enters SNAPSHOT")
	await _wait_for_complete()
	_assert(bool(film.get("ProbeViewAvailable")), "formal Snapshot seals authority")
	_assert(int(film.get("SealedProbeViewGeneration")) == 1, "first public Snapshot generation")
	var generation := int(film.get("SealedProbeViewGeneration"))

	_send_key(KEY_F)
	await _settle(4)
	_assert(bool(film.get("ProbeViewAvailable")), "F does not invalidate seal")
	_assert(int(film.get("SealedProbeViewGeneration")) == generation, "F preserves generation")
	_send_key(KEY_F)
	await _settle(4)

	_send_key(KEY_Q)
	await _settle(2)
	_assert(int(film.get("CurrentProbeView")) == 1, "Q maps Outcome to Contact Events")
	_send_key(KEY_Q)
	await _settle(2)
	_assert(int(film.get("CurrentProbeView")) == 2, "Q maps Contact Events to Effort")
	# Shift+Q is represented as a modified Q event by the routed input path.
	var reverse := InputEventKey.new()
	reverse.keycode = KEY_Q
	reverse.physical_keycode = KEY_Q
	reverse.pressed = true
	reverse.echo = false
	reverse.shift_pressed = true
	Input.parse_input_event(reverse)
	await _settle(2)
	_assert(int(film.get("CurrentProbeView")) == 1, "Shift+Q maps Effort back to Contact Events")
	_assert(int(film.get("SealedProbeViewGeneration")) == generation, "Q preserves generation")

	_send_key(KEY_E)
	await _settle(6)
	_assert(not bool(film.get("ProbeViewAvailable")), "E invalidates sealed authority")
	_assert(str(film.get("CathedralSnapshotLifecycleStateName")) == "invalidated", "E invalidation state")
	_assert(not bool(chamber.get_node("TransportChamberPlayer").call("IsPoseHeld")), "E releases held pose")

	_send_key(KEY_G)
	await _settle(4)
	_assert(film_controller.call("GetModeName") == "LIVE", "Snapshot to LIVE transition")
	_assert(not bool(film.get("CathedralSnapshotLifecycleActive")), "LIVE does not overlap Snapshot owner")
	_send_key(KEY_G)
	await _settle(2)
	_assert(film_controller.call("GetModeName") == "OFF", "LIVE to OFF transition")
	_send_key(KEY_G)
	await _settle(2)
	_assert(film_controller.call("GetModeName") == "SNAPSHOT", "second G enters new Snapshot")
	await _wait_for_complete()
	_assert(bool(film.get("ProbeViewAvailable")), "second Snapshot seals authority")
	var second_generation := int(film.get("SealedProbeViewGeneration"))
	_assert(second_generation > generation, "second public Snapshot generation is newer")
	print("PUBLIC CATHEDRAL SNAPSHOT PATH PASS generation=%d" % second_generation)
	quit(0)

func _wait_for_complete() -> void:
	for _i in range(600):
		await process_frame
		if bool(film.get("ProbeViewAvailable")):
			return
	_fail("formal Snapshot timeout")

func _all_field_sources_visible() -> bool:
	for source in scene_root.get_tree().get_nodes_in_group("field_sources"):
		if bool(source.get("DebugVizEnabled")) and not bool(source.get("DebugVizInGame")):
			return false
	return true

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
		_fail(message)

func _fail(message: String) -> void:
	push_error("PUBLIC CATHEDRAL SNAPSHOT PATH FAIL: " + message)
	quit(1)
