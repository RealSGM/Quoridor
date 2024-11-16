class_name Board extends Control

@export_category("Board")
@export var tile_container: GridContainer
@export var fence_button_container: GridContainer


## Setup the board with the selected size
func setup_board(board_size: int) -> void:
	instance_tiles(board_size)
	instance_fence_buttons(board_size - 1)
	tile_container.columns = board_size


## Set the grid container size and instance the tiles under the grid
func instance_tiles(board_size) -> void:
	var tile_resource: Resource  = Resources.get_resource('tile')
	for i: int in range(board_size * board_size):
		var tile: Tile = tile_resource.instantiate()
		tile.set_disabled(true)
		tile_container.add_child(tile, true)

## Set the fence container size and instance the fence buttons under the 
## container
func instance_fence_buttons(fence_size) -> void:
	fence_button_container.columns = fence_size
	var fence_button_resource: Resource  = Resources.get_resource('fence_button')
	for i: int in range(fence_size * fence_size):
		var fence_button: Button = fence_button_resource.instantiate()
		fence_button_container.add_child(fence_button, true)
