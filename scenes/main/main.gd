extends Control

@export var board_size: int = 9

@export_category("Nodes")
@export var grid_container: GridContainer

@onready var tile_resource: Resource = Resources.get_resource('tile')
 
func _ready() -> void:
	setup_board()


func setup_board() -> void:
	grid_container.columns = board_size
	
	for i: int in range(board_size * board_size):
		var tile: Control = tile_resource.instantiate()
		grid_container.add_child(tile, true)
