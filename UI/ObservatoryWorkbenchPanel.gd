extends PanelContainer

const RECIPE_ID := "observer_instrumentation_diagnostics"
const FIXTURE_IDS := ["oi_001", "oi_006", "oi_012"]
const FIXTURE_LABELS := ["OI-001", "OI-006", "OI-012"]
const FIXTURE_NAMES := ["Equator UV Band", "Checker Diagnostic", "Texture Sample"]
const FIXTURE_PNGS := [
	"",
	"artifacts/oi_007_checker_diagnostic_upscaled.png",
	"artifacts/oi_012_texture_sample_diagnostic_upscaled.png",
]
const FIXTURE_PNGS_FALLBACK := [
	"",
	"artifacts/oi_006_checker_diagnostic.png",
	"artifacts/oi_012_texture_sample_diagnostic.png",
]

@onready var _controller: Node = $TestBenchController
@onready var _status_chip: Label = $Margin/Root/Header/StatusChip
@onready var _run_button: Button = $Margin/Root/Actions/RunButton
@onready var _stop_button: Button = $Margin/Root/Actions/StopButton
@onready var _open_bundle_button: Button = $Margin/Root/Actions/OpenBundleButton
@onready var _open_report_button: Button = $Margin/Root/Actions/OpenReportButton
@onready var _detail_header: Label = $Margin/Root/DetailHeader
@onready var _preview_texture: TextureRect = $Margin/Root/PreviewArea/PreviewTexture
@onready var _provenance_log: TextEdit = $Margin/Root/ProvenanceLog

@onready var _fixture_chips: Array = [
	$Margin/Root/Experiments/Oi001Row/Oi001Chip,
	$Margin/Root/Experiments/Oi006Row/Oi006Chip,
	$Margin/Root/Experiments/Oi012Row/Oi012Chip,
]
@onready var _fixture_btns: Array = [
	$Margin/Root/Experiments/Oi001Row/Oi001Btn,
	$Margin/Root/Experiments/Oi006Row/Oi006Btn,
	$Margin/Root/Experiments/Oi012Row/Oi012Btn,
]

var _bundle_dir := ""
var _verdicts := ["-", "-", "-"]
var _selected := 0
var _is_running := false
var _poll_timer: Timer
var _preview_path := ""


func _ready() -> void:
	_setup_poll_timer()
	_run_button.pressed.connect(_on_run_pressed)
	_stop_button.pressed.connect(_on_stop_pressed)
	_open_bundle_button.pressed.connect(_on_open_bundle_pressed)
	_open_report_button.pressed.connect(_on_open_report_pressed)
	for idx in FIXTURE_IDS.size():
		var btn: Button = _fixture_btns[idx]
		btn.pressed.connect(_on_fixture_selected.bind(idx))
	_set_state_idle()
	_fixture_btns[0].button_pressed = true
	_on_fixture_selected(0)


func _setup_poll_timer() -> void:
	_poll_timer = Timer.new()
	_poll_timer.wait_time = 0.35
	_poll_timer.one_shot = false
	_poll_timer.timeout.connect(_poll)
	add_child(_poll_timer)
	_poll_timer.start()


func _on_run_pressed() -> void:
	_bundle_dir = ""
	_verdicts = ["-", "-", "-"]
	var result = JSON.parse_string(str(_controller.call(
		"RunJson", RECIPE_ID, "smoke", "none", "none"
	)))
	if typeof(result) != TYPE_DICTIONARY:
		_status_chip.text = "Error: bad response"
		return
	var manifest: Dictionary = result.get("manifest", {})
	_bundle_dir = str(manifest.get("planned_output_folder", ""))
	_set_state_running()


func _on_stop_pressed() -> void:
	_controller.call("StopRun")


func _on_open_bundle_pressed() -> void:
	_controller.call("OpenLatestFolder")


func _on_open_report_pressed() -> void:
	_controller.call("OpenReport")


func _poll() -> void:
	if _controller == null:
		return
	var log_chunk := str(_controller.call("DrainLog"))
	if not log_chunk.is_empty() and _is_running:
		_provenance_log.text += log_chunk
		_provenance_log.scroll_vertical = _provenance_log.get_line_count()
		_apply_live_chips(log_chunk)

	var status = JSON.parse_string(str(_controller.call("GetStatusJson")))
	if typeof(status) != TYPE_DICTIONARY:
		return
	var running: bool = bool(status.get("running", false))
	if not running and _is_running:
		_finalize()


func _apply_live_chips(log_chunk: String) -> void:
	for idx in FIXTURE_IDS.size():
		var label: String = FIXTURE_LABELS[idx]
		if log_chunk.contains("[%s] PASS" % label):
			_verdicts[idx] = "PASS"
		elif log_chunk.contains("[%s] FAIL" % label):
			_verdicts[idx] = "FAIL"
	_refresh_chips()


func _finalize() -> void:
	_is_running = false
	if _bundle_dir.is_empty():
		_set_state_error("No bundle dir.")
		return

	var summary_path := _bundle_dir + "/summary.json"
	if not FileAccess.file_exists(summary_path):
		_set_state_error("summary.json missing.")
		return

	var f := FileAccess.open(summary_path, FileAccess.READ)
	if f == null:
		_set_state_error("Cannot open summary.json.")
		return
	var summary = JSON.parse_string(f.get_as_text())
	f = null
	if typeof(summary) != TYPE_DICTIONARY:
		_set_state_error("summary.json unreadable.")
		return

	var fixtures: Dictionary = summary.get("fixtures", {})
	for idx in FIXTURE_IDS.size():
		var fid: String = FIXTURE_IDS[idx]
		if fixtures.has(fid):
			_verdicts[idx] = str(fixtures[fid].get("verdict", "-"))
		else:
			_verdicts[idx] = "-"

	var overall := str(summary.get("overall_verdict", "-"))
	if overall == "PASS":
		_status_chip.text = "PASS"
		_status_chip.add_theme_color_override("font_color", Color(0.45, 0.95, 0.58))
	else:
		_status_chip.text = overall
		_status_chip.add_theme_color_override("font_color", Color(1.0, 0.38, 0.32))

	_run_button.disabled = false
	_stop_button.disabled = true
	_open_bundle_button.disabled = false
	_open_report_button.disabled = false
	_refresh_chips()
	_refresh_detail_panel(_selected)


func _on_fixture_selected(idx: int) -> void:
	_selected = idx
	_refresh_detail_panel(idx)


func _refresh_detail_panel(idx: int) -> void:
	var label: String = FIXTURE_LABELS[idx]
	var verdict: String = _verdicts[idx]
	_detail_header.text = "%s - %s" % [label, FIXTURE_NAMES[idx]]
	if verdict != "-":
		_detail_header.text += " [%s]" % verdict

	var png_path := ""
	if not _bundle_dir.is_empty():
		var primary: String = _bundle_dir + "/" + FIXTURE_PNGS[idx]
		var fallback: String = _bundle_dir + "/" + FIXTURE_PNGS_FALLBACK[idx]
		if not FIXTURE_PNGS[idx].is_empty() and FileAccess.file_exists(primary):
			png_path = primary
		elif not FIXTURE_PNGS_FALLBACK[idx].is_empty() and FileAccess.file_exists(fallback):
			png_path = fallback
	_update_preview(png_path)

	var log_path := ""
	if not _bundle_dir.is_empty():
		log_path = _bundle_dir + "/logs/" + FIXTURE_IDS[idx] + ".log"
	if not _is_running and not log_path.is_empty() and FileAccess.file_exists(log_path):
		var f := FileAccess.open(log_path, FileAccess.READ)
		if f != null:
			_provenance_log.text = f.get_as_text()
			_provenance_log.scroll_vertical = _provenance_log.get_line_count()
			f = null


func _update_preview(path: String) -> void:
	if path.is_empty():
		_preview_path = ""
		_preview_texture.texture = null
		return
	if path == _preview_path:
		return
	_preview_path = path
	var image := Image.new()
	var err := image.load(path)
	if err != OK:
		_preview_texture.texture = null
		return
	_preview_texture.texture = ImageTexture.create_from_image(image)


func _refresh_chips() -> void:
	for idx in FIXTURE_IDS.size():
		var chip: Label = _fixture_chips[idx]
		chip.text = _verdicts[idx]
		chip.remove_theme_color_override("font_color")
		match _verdicts[idx]:
			"PASS":
				chip.add_theme_color_override("font_color", Color(0.45, 0.95, 0.58))
			"FAIL":
				chip.add_theme_color_override("font_color", Color(1.0, 0.38, 0.32))
			"...":
				chip.add_theme_color_override("font_color", Color(1.0, 0.82, 0.35))


func _set_state_idle() -> void:
	_is_running = false
	_status_chip.text = "Ready"
	_status_chip.remove_theme_color_override("font_color")
	_run_button.disabled = false
	_stop_button.disabled = true
	_open_bundle_button.disabled = _bundle_dir.is_empty()
	_open_report_button.disabled = _bundle_dir.is_empty()


func _set_state_running() -> void:
	_is_running = true
	_status_chip.text = "Running..."
	_status_chip.remove_theme_color_override("font_color")
	_run_button.disabled = true
	_stop_button.disabled = false
	_open_bundle_button.disabled = true
	_open_report_button.disabled = true
	_verdicts = ["...", "...", "..."]
	_refresh_chips()
	_provenance_log.text = ""


func _set_state_error(msg: String) -> void:
	_status_chip.text = "Error"
	_status_chip.add_theme_color_override("font_color", Color(1.0, 0.38, 0.32))
	_run_button.disabled = false
	_stop_button.disabled = true
	_provenance_log.text += "\n[error] " + msg
