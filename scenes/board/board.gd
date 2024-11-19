class_name Board extends Control

# TODO Disable the tile that the pawn is currently on
# TODO Spawn "ghost" pawns for both players on every tile

# TODO Pawn Movement
# - Clicking the pawn shows all possible tiles it can move to
# - Clicking adjacent tile show translucent pawn on tile
# - Click confirm to move the pawn
# - Clicking a fence button or the pawn again (instead of confirm) cancels it

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
	spawn_pawns(board_size)


## Set the grid container size and instance the tiles under the grid
func instance_tiles(board_size: int) -> void:
	var tile_resource: Resource  = Resources.get_resource('tile')
	var total_tiles: int = board_size * board_size
	tile_container.columns = board_size
	
	for i: int in range(total_tiles):
		var tile: Tile = tile_resource.instantiate()
		#tile.set_disabled(true)
		tile_container.add_child(tile, true)
		tile_buttons.append(tile)
	
	for index: int in range(total_tiles):
		var curr_tile: Tile = tile_buttons[index]
		
		if index >= board_size: # Check for North Tile
			curr_tile.connections[0] = tile_buttons[index - board_size]
		if (index + 1) % board_size != 0: # Check for East Tile
			curr_tile.connections[1] = tile_buttons[index + 1]
		if index < total_tiles - board_size: # Check for South Tile
			curr_tile.connections[2] = tile_buttons[index + board_size]
		if index % board_size != 0: # Check for West Tile
			curr_tile.connections[3] = tile_buttons[index - 1]


## Set the fence container size and instance the fence buttons under the 
## container
func instance_fence_buttons(fence_size: int) -> void:
	var fence_button_resource: Resource  = Resources.get_resource('fence_button')
	var total_fences: int = fence_size * fence_size
	fence_button_container.columns = fence_size
	
	for i: int in range(fence_size * fence_size):
		var fence_button: Button = fence_button_resource.instantiate()
		fence_buttons.append(fence_button)
		fence_button_container.add_child(fence_button, true)
	
	for index: int in range(total_fences):
		var curr_fence: FenceButton = fence_buttons[index]
		
		if index >= fence_size: # Check for North Tile
			curr_fence.connections[0] = fence_buttons[index - fence_size]
		if (index + 1) % fence_size != 0: # Check for East Tile
			curr_fence.connections[1] = fence_buttons[index + 1]
		if index < total_fences - fence_size: # Check for South Tile
			curr_fence.connections[2] = fence_buttons[index + fence_size]
		if index % fence_size != 0: # Check for West Tile
			curr_fence.connections[3] = fence_buttons[index - 1]


func set_confirm_button(fence_button: FenceButton, tile_button: Tile) -> void:
	confirm_button.disabled = !(fence_button || tile_button)


func update_fence_buttons() -> void:
	for fence_button: FenceButton in fence_buttons:
		fence_button.disabled = fence_button.dir_is_disabled[Global.dir_index]
		# Disable mouse filter if the button is disabled
		fence_button.mouse_filter = Control.MOUSE_FILTER_IGNORE if fence_button.disabled else Control.MOUSE_FILTER_STOP
	# TODO Implement illegal fence check


@warning_ignore("integer_division")
@warning_ignore("narrowing_conversion")
func spawn_pawns(board_size: int) -> void:
	spawn_pawn(Global.player_one["name"], Global.player_one["color"], board_size / 2)
	spawn_pawn(Global.player_two["name"], Global.player_two["color"], board_size * (board_size - 0.5))


func spawn_pawn(p_name: String, color: Color, pos_index: int) -> void:
	var pawn: Pawn = Resources.get_resource("pawn").instantiate()
	pawn.colour = color
	pawn.player_name = p_name
	tile_buttons[pos_index].add_child(pawn, true)

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
	update_fence_buttons()


func _on_confirm_pressed() -> void:
	if selected_fence_button:
		# Flip the index (for NESW adjustment)
		var flipped_index: int = 1 - Global.dir_index
		# Get the adjacent directionals
		var disabled_indexes: Array[int] = [flipped_index, flipped_index + 2]
		
		# Disable the adjacents buttons, for that direction
		for indexes: int in disabled_indexes:
			if selected_fence_button.connections[indexes]:
				selected_fence_button.connections[indexes].dir_is_disabled[Global.dir_index] = true
		selected_fence_button.fence_placed = true
		selected_fence_button = null
	
	# TODO Implemement pawn movement
	elif selected_pawn_tile:
		pass
	
	update_fence_buttons()
