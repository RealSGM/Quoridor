class_name Board extends Control

# TODO - Add the connections for each fence, both vertical and horizontal
# TODO - Disable fence buttons depending if it has a horizontal / vertical button

@export_category("Board")
@export var board_anchor: Control
@export var tile_container: GridContainer
@export var fence_button_container: GridContainer

@export_category("User Interface")
@export var horizontal_button: Button
@export var vertical_button: Button
@export var confirm_button: Button
@export var exit_button: Button

var fence_buttons: Array[FenceButton] = []
var tile_buttons: Array[Tile] = []

@onready var dir_buttons: Array[Button] = [horizontal_button, vertical_button]
@onready var selected_fence_button: FenceButton = null:
	set(val):
		if selected_fence_button:
			selected_fence_button.clear_fences()
		selected_fence_button = val
		set_confirm_button(val, null) # TODO Replace null with the current selected tile
# TODO Future - Pawn Movement
@onready var selected_pawn_tile: Tile = null:
	set(val):
		selected_pawn_tile = val


func _ready() -> void:
	_on_directional_button_pressed()
	exit_button.pressed.connect(SignalManager.exit_pressed.emit)

## Setup the board with the selected size
func setup_board(board_size: int) -> void:
	instance_tiles(board_size)
	instance_fence_buttons(board_size - 1)
	tile_container.columns = board_size


## Set the grid container size and instance the tiles under the grid
func instance_tiles(board_size: int) -> void:
	var tile_resource: Resource  = Resources.get_resource('tile')
	var total_tiles: int = board_size * board_size
	
	for i: int in range(total_tiles):
		var tile: Tile = tile_resource.instantiate()
		tile.set_disabled(true)
		tile_container.add_child(tile, true)
		tile_buttons.append(tile)
	
	for index: int in range(total_tiles):
		var curr_tile: Tile = tile_buttons[index]
		
		# Check for North Tile
		if index >= board_size:
			curr_tile.connections[0] = tile_buttons[index - board_size]
		# Check for East Tile
		if (index + 1) % board_size != 0:
			curr_tile.connections[1] = tile_buttons[index + 1]
		# Check for South Tile
		if index < total_tiles - board_size:
			curr_tile.connections[2] = tile_buttons[index + board_size]
		# Check for West Tile
		if index % board_size != 0:
			curr_tile.connections[3] = tile_buttons[index - 1]

## Set the fence container size and instance the fence buttons under the 
## container
func instance_fence_buttons(fence_size: int) -> void:
	fence_button_container.columns = fence_size
	var fence_button_resource: Resource  = Resources.get_resource('fence_button')
	for i: int in range(fence_size * fence_size):
		var fence_button: Button = fence_button_resource.instantiate()
		fence_buttons.append(fence_button)
		fence_button_container.add_child(fence_button, true)


func set_confirm_button(fence_button: FenceButton, tile_button: Tile) -> void:
	confirm_button.disabled = !(fence_button || tile_button)


# Signals ----------------------------------------------------------------------

func _on_fence_button_pressed(fence_button: FenceButton) -> void:
	selected_fence_button = fence_button
	fence_button.h_fence.visible = Global.dir_index == 0
	fence_button.v_fence.visible = Global.dir_index != 0


## Flip the rotation of the fence
func _on_directional_button_pressed() -> void:
	selected_fence_button = null
	dir_buttons[Global.dir_index].disabled = false
	Global.dir_index = 1 - Global.dir_index
	dir_buttons[Global.dir_index].disabled = true
	
	# TODO Loop through all fence buttons and check if with the current direction
	# whether it should be disabled or not


func _on_confirm_pressed() -> void:
	if selected_fence_button:
		selected_fence_button.fence_placed = true
		selected_fence_button = null
		# TODO Null the tile connections for the fence being placed
	elif selected_pawn_tile:
		pass
