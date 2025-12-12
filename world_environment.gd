extends WorldEnvironment

@export var beta := 0.0
@export var gamma := 0.0

# Called when the node enters the scene tree for the first time.
func _ready():
	var mat := ShaderMaterial.new()
	mat.shader = load("res://curved_ray_postprocess.gdshader")
	environment.volumetric_fog_density = 0.0
	environment.background_mode = Environment.BG_SKY

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
