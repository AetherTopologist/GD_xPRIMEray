extends Node

@onready var _tabs: TabContainer = $UILayer/UIRoot/SideDock/Tabs
@onready var _side_dock: Control = $UILayer/UIRoot/SideDock
@onready var _hint_text: Label = $UILayer/UIRoot/LaunchHintPanel/HintMargin/HintText

var _workbench_open := false
var _input_camera: Node


func _ready() -> void:
	_tabs.current_tab = 0
	_tabs.set_tab_title(0, "Observatory")
	_tabs.set_tab_title(1, "TestBench")
	_input_camera = _find_input_camera(self)
	_set_workbench_open(false)


func _input(event: InputEvent) -> void:
	if event is InputEventKey and event.pressed and not event.echo:
		if event.keycode == KEY_ESCAPE:
			_set_workbench_open(not _workbench_open)
			get_viewport().set_input_as_handled()


func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		if _workbench_open:
			_set_workbench_open(false)
			get_viewport().set_input_as_handled()
		elif Input.mouse_mode != Input.MOUSE_MODE_CAPTURED:
			Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
			_update_hint()


func _set_workbench_open(open: bool) -> void:
	_workbench_open = open
	_side_dock.visible = open
	if _input_camera != null and _input_camera.has_method("SetInputEnabled"):
		_input_camera.call("SetInputEnabled", not open, true)
	Input.mouse_mode = Input.MOUSE_MODE_VISIBLE if open else Input.MOUSE_MODE_CAPTURED
	_update_hint()


func _update_hint() -> void:
	if _workbench_open:
		_hint_text.text = "Observatory open · Esc or click world to resume movement"
	elif Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		_hint_text.text = "WASD move · Mouse look · Esc for Observatory"
	else:
		_hint_text.text = "Click to explore · Esc for Observatory"


func _find_input_camera(root: Node) -> Node:
	if root.has_method("SetInputEnabled"):
		return root
	for child in root.get_children():
		var found := _find_input_camera(child)
		if found != null:
			return found
	return null
