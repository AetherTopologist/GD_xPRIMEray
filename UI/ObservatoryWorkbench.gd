extends Node

@onready var _tabs: TabContainer = $UILayer/UIRoot/SideDock/Tabs


func _ready() -> void:
	_tabs.current_tab = 0
	_tabs.set_tab_title(0, "Observatory")
	_tabs.set_tab_title(1, "TestBench")
