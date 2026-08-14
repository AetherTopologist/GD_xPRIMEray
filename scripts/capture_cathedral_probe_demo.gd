extends SceneTree

var root_scene: Node
var film: Node
var renderer: Node
var configured := false
var requested := false
var ticks := 0

func _init() -> void:
	root_scene = load("res://ObservatoryWorkbench.tscn").instantiate()
	get_root().add_child(root_scene)
	root_scene.process_mode = Node.PROCESS_MODE_DISABLED

func _process(_delta: float) -> bool:
	ticks += 1
	if not configured:
		film = root_scene.get_node("PlayableWorld/TransportChamberWorld/GrinFilmCamera")
		renderer = root_scene.get_node("PlayableWorld/TransportChamberWorld/RayBeamRenderer")
		var chamber: Node = root_scene.get_node("PlayableWorld/TransportChamberWorld")
		var player: Node = chamber.get_node("TransportChamberPlayer")
		player.SetInputEnabled(false, true)
		player.set_physics_process(false)
		chamber.set_process(false)
		chamber.get_node("FilmController").set_process(false)
		chamber.get_node("FieldDialController").set_process(false)
		renderer.StepsPerRay = int(OS.get_environment("CAPTURE_STEPS"))
		renderer.StepLength = float(OS.get_environment("CAPTURE_STEP_LENGTH"))
		renderer.FieldStrength = float(OS.get_environment("CAPTURE_FIELD"))
		var dimensions: PackedStringArray = OS.get_environment("CAPTURE_FILM").split("x")
		film.Width = int(dimensions[0])
		film.Height = int(dimensions[1])
		film.FilmResolutionScale = 1.0
		film.PixelStride = 1
		film.RowsPerFrame = 1
		film.MaxRowsPerFrameCap = 1
		film.UpdateEveryFrame = false
		film.RenderStepMaxMs = 120
		root_scene.process_mode = Node.PROCESS_MODE_INHERIT
		configured = true
		return false
	if not requested:
		film.RequestCathedralProbeSnapshot(240)
		requested = true
		return false
	if not film.CathedralProbeSnapshotIsTerminal:
		if ticks > 300:
			printerr("[CaptureBundle] timeout")
			quit(2)
		return false
	var output_dir: String = OS.get_environment("CAPTURE_OUTPUT")
	var run_id: String = OS.get_environment("CAPTURE_RUN_ID")
	var scene_id: String = OS.get_environment("CAPTURE_SCENE_ID")
	var engine_commit: String = OS.get_environment("CAPTURE_ENGINE_COMMIT")
	var failure: String = film.CapturePortableObservatoryBundle(
		output_dir,
		run_id,
		scene_id,
		"res://ObservatoryWorkbench.tscn",
		engine_commit,
		"8024e206,75191004,d6211f0f,94dff98b,af71c50e")
	if not failure.is_empty():
		printerr("[CaptureBundle] failed: ", failure)
		quit(3)
	print("[CaptureBundle] complete output=", output_dir)
	quit()
	return false
