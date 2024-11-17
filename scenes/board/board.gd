class_name Board extends Control

@export_category("Board")
@export var board_anchor: Control
@export var tile_container: GridContainer
@export var fence_button_container: GridContainer
@export var horizontal_button: Button
@export var vertical_button: Button

@onready var dir_buttons: Array[Button] = [horizontal_button, vertical_button]
var selected_fence_button: FenceButton = null

## Setup the board with the selected size
func setup_board(board_size: int) -> void:
	instance_tiles(board_size)
	instance_fence_buttons(board_size - 1)
	tile_container.columns = board_size
	
	Global.dir_index = 0
	horizontal_button.disabled = true
	vertical_button.disabled = false


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


func _on_fence_button_pressed(fence_button: FenceButton) -> void:
	if selected_fence_button:
		selected_fence_button.clear_fences()
	
	selected_fence_button = fence_button
	fence_button.h_fence.visible = Global.dir_index == 0
	fence_button.v_fence.visible = Global.dir_index != 0


## Flip the rotation of the fence
func _on_directional_button_pressed() -> void:
	dir_buttons[Global.dir_index].disabled = false
	Global.dir_index = 1 - Global.dir_index
	dir_buttons[Global.dir_index].disabled = true


func _on_confirm_pressed() -> void:
	pass # Replace with function body.
