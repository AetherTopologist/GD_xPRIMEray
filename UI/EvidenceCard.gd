class_name EvidenceCard
extends PanelContainer

# Generic evidence card component. Accepts structured data from the caller;
# has no knowledge of OI fixture IDs, log formats, or bundle layout.

@onready var _card_title: Label       = $CardMargin/CardRoot/CardHeader/CardTitle
@onready var _card_verdict: Label     = $CardMargin/CardRoot/CardHeader/CardVerdict
@onready var _claim_text: Label       = $CardMargin/CardRoot/ClaimSection/ClaimText
@onready var _boundary_text: Label    = $CardMargin/CardRoot/BoundarySection/BoundaryText
@onready var _why_section: VBoxContainer = $CardMargin/CardRoot/WhySection
@onready var _why_text: Label         = $CardMargin/CardRoot/WhySection/WhyText
@onready var _legacy_chip: Label      = $CardMargin/CardRoot/MetricsSection/MetricsHeader/LegacyChip
@onready var _metrics_container: VBoxContainer = $CardMargin/CardRoot/MetricsSection/MetricsContainer
@onready var _preview_area: PanelContainer = $CardMargin/CardRoot/PreviewArea
@onready var _preview_texture: TextureRect = $CardMargin/CardRoot/PreviewArea/PreviewTexture
@onready var _log_toggle: Button      = $CardMargin/CardRoot/LogSection/LogHeader/LogToggle
@onready var _provenance_log: TextEdit = $CardMargin/CardRoot/LogSection/ProvenanceLog

var _preview_path := ""


func _ready() -> void:
	_log_toggle.pressed.connect(_toggle_log)


# --- Public API ---

# Full display pass. Accepts a Dictionary with the following keys:
#
#   title         : String   — shown in card header
#   verdict       : String   — "PASS" / "FAIL" / "..." / "-" / "Error"
#   claim         : String   — one-sentence claim
#   boundary      : String   — one-sentence boundary
#   why_it_matters: String   — optional; section hidden when empty or absent
#   metrics       : Array    — [{key: String, value: String}, ...] display rows
#   legacy_metrics: bool     — true → "legacy" chip visible beside METRICS label
#   artifact_path : String   — filesystem path to PNG; empty → preview hidden
#   log_text      : String   — full log content for the provenance log
#
# Does not auto-expand the log; caller must call set_log_visible(true) when appropriate.
func populate(data: Dictionary) -> void:
	_card_title.text = str(data.get("title", ""))

	var verdict: String = str(data.get("verdict", "-"))
	_card_verdict.text = verdict
	_card_verdict.remove_theme_color_override("font_color")
	match verdict:
		"PASS":  _card_verdict.add_theme_color_override("font_color", Color(0.45, 0.95, 0.58))
		"FAIL":  _card_verdict.add_theme_color_override("font_color", Color(1.0, 0.38, 0.32))
		"...":   _card_verdict.add_theme_color_override("font_color", Color(1.0, 0.82, 0.35))
		"Error": _card_verdict.add_theme_color_override("font_color", Color(1.0, 0.38, 0.32))

	_claim_text.text = str(data.get("claim", ""))
	_boundary_text.text = str(data.get("boundary", ""))

	var why: String = str(data.get("why_it_matters", ""))
	_why_section.visible = not why.is_empty()
	_why_text.text = why

	_rebuild_metrics(data.get("metrics", []))
	_legacy_chip.visible = bool(data.get("legacy_metrics", false))

	_load_preview(str(data.get("artifact_path", "")))

	_provenance_log.text = str(data.get("log_text", ""))


# Append live log text during a run (does not affect other card fields).
func append_log(chunk: String) -> void:
	_provenance_log.text += chunk
	_provenance_log.scroll_vertical = _provenance_log.get_line_count()


# Show or hide the log panel without toggling (used to force-expand on error).
func set_log_visible(visible_state: bool) -> void:
	_provenance_log.visible = visible_state
	_log_toggle.text = "v Log" if visible_state else "> Log"


# Clear all card content and reset to the start-of-run neutral state.
func reset_for_run() -> void:
	_card_verdict.text = "..."
	_card_verdict.add_theme_color_override("font_color", Color(1.0, 0.82, 0.35))
	_claim_text.text = ""
	_boundary_text.text = ""
	_why_section.visible = false
	_rebuild_metrics([])
	_legacy_chip.visible = false
	_preview_area.visible = false
	_preview_texture.texture = null
	_preview_path = ""
	_provenance_log.text = ""
	_provenance_log.visible = false
	_log_toggle.text = "> Log"


# Append an error message and force-expand the log.
func show_error(msg: String) -> void:
	_card_verdict.text = "Error"
	_card_verdict.add_theme_color_override("font_color", Color(1.0, 0.38, 0.32))
	_rebuild_metrics([])
	_legacy_chip.visible = false
	_provenance_log.text += "\n[error] " + msg
	set_log_visible(true)


# --- Private ---

func _rebuild_metrics(rows: Array) -> void:
	for child in _metrics_container.get_children():
		child.queue_free()
	for row in rows:
		var hbox := HBoxContainer.new()
		var key_lbl := Label.new()
		var val_lbl := Label.new()
		key_lbl.text = str(row.get("key", ""))
		key_lbl.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		key_lbl.add_theme_font_size_override("font_size", 11)
		val_lbl.text = str(row.get("value", ""))
		val_lbl.add_theme_font_size_override("font_size", 11)
		hbox.add_child(key_lbl)
		hbox.add_child(val_lbl)
		_metrics_container.add_child(hbox)


func _load_preview(path: String) -> void:
	_preview_area.visible = not path.is_empty()
	if path.is_empty():
		_preview_path = ""
		_preview_texture.texture = null
		return
	if path == _preview_path:
		return
	_preview_path = path
	var image := Image.new()
	if image.load(path) != OK:
		_preview_texture.texture = null
		return
	_preview_texture.texture = ImageTexture.create_from_image(image)


func _toggle_log() -> void:
	set_log_visible(not _provenance_log.visible)
