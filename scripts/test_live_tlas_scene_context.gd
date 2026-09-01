extends SceneTree

func _initialize() -> void:
	change_scene_to_file("res://ObservatoryWorkbench.tscn")
	await scene_changed
	await process_frame
	var film: Node = current_scene.get_node("PlayableWorld/TransportChamberWorld/GrinFilmCamera")
	film.set("DebugSnapshotLog", true)
	film.set("UpdateEveryFrame", true)
	for _i in range(30):
		await process_frame
	print("LIVE_TLAS_SCENE_CONTEXT PASS current_scene=%s" % current_scene.name)
	quit(0)
