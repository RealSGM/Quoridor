class_name Board extends Control

# TODO Player Block Leaping
# TODO Win Scenario
# TODO Illegal Fence Check (C#)

@export_category("Board")
@export var board_anchor: Control
@export var tile_container: GridContainer
@export var board_container: PanelContainer
@export var fence_button_container: GridContainer

@export_category("User Interface")
@export var toggle_direction_button: Button
@export var move_pawn_button: Button
@export var place_fence_button: Button
@export var confirm_button: Button
@export var exit_button: Button
@export var turn_label: Label

var fence_buttons: Array[FenceButton] = []
var tile_buttons: Array[Tile] = []

## Stored as [Player One Tile, Player Two Tile]
var current_tiles: Array[Tile] = [null, null]
var last_choice: Callable = _on_move_pawn_pressed
var directions: Array[int] = []

@onready var selected_fence_button: FenceButton = null:
	set(val):
		# Clear current fence
		if selected_fence_button:
			selected_fence_button.clear_fences()
		
		# Validate the confirm button
		selected_fence_button = val
		set_confirm_button(val, selected_pawn_tile)

@onready var selected_pawn_tile: Tile = null:
	set(val):
		# Clear the current pawn
		if selected_pawn_tile:
			selected_pawn_tile.clear_pawns()
		
		# Validate the confirm button
		selected_pawn_tile = val
		set_confirm_button(selected_fence_button, val)
		
		if val:
			var pawn: Pawn = val.pawns[current_player]
			pawn.modulate.a = 0.5
			pawn.show()

@onready var current_player: int:
	set(val):
		current_player = val
		last_choice.call()
		turn_label.text = str(Global.players[current_player]["name"]) + "'s Turn"

func _ready() -> void:
	_on_directional_button_pressed()
	current_player = 0
	
	exit_button.pressed.connect(SignalManager.exit_pressed.emit)
	SignalManager.tile_pressed.connect(func(tile: Tile) -> void: selected_pawn_tile = tile)


## Setup the board with the selected size
func setup_board(board_size: int) -> void:
	Global.board_size = board_size
	directions = [-Global.board_size, 1, Global.board_size, 1]
	instance_tiles(board_size)
	instance_fence_buttons(board_size - 1)
	spawn_pawns(board_size)
	reset_board(Color.TRANSPARENT)


func set_confirm_button(fence_button: FenceButton, tile_button: Tile) -> void:
	confirm_button.disabled = !(fence_button || tile_button)


## Disable all tiles, and reset their modulate
func reset_board(color: Color) -> void:
	tile_buttons.map(func(tile: Tile): 
		tile.disabled = true
		tile.modulate = Color.WHITE
	)
	set_fence_buttons(color)


# Tiles ------------------------------------------------------------------------
## Set the grid container size and instance the tiles under the grid
func instance_tiles(board_size: int) -> void:
	var tile_resource: Resource  = Resources.get_resource('tile')
	var total_tiles: int = board_size * board_size
	tile_container.columns = board_size
	
	# Instance all tiles onto the board
	for i: int in range(total_tiles):
		var tile: Tile = tile_resource.instantiate()
		tile_container.add_child(tile, true)
		tile_buttons.append(tile)
	
	# Set each tile's connection
	for index: int in range(total_tiles):
		var curr_tile: Tile = tile_buttons[index]
		curr_tile.set_connections(index, board_size, tile_buttons)


@warning_ignore('incompatible_ternary')
func set_non_adjacent_tiles(current_tile: Tile, set_disabled: bool) -> void:
	var tiles_to_enable: Array[Tile] = []
	
	for tile: Tile in tile_buttons:
		# Disable the current tile, but do not darken
		if tile == current_tile:
			tile.disabled = true
			
		# Enable the current tiles
		elif tile in current_tile.connections:
			# Check if the tile is taken by enemy pawn:
			if tile == current_tiles[1 - current_player]:
				fully_disable_tile(tile, set_disabled)
				# Check if the tile infront is leapable
				# Check if a fence is blocking it
				
				# Get the cardinal direction
				var player_index: int = tile_buttons.find(current_tile)
				var index: int = get_direction(player_index, tile)
				var leaped_tile: Tile = tile_buttons[player_index + (directions[index] * 2)]
				tiles_to_enable.append(leaped_tile)
			else:
				tiles_to_enable.append(tile)
				
		else:
			fully_disable_tile(tile, set_disabled)
	
	# Enable all tiles
	tiles_to_enable.map(func(tile: Tile):
		tile.disabled = false
		tile.modulate = Color.WHITE
	)


func fully_disable_tile(tile: Tile, set_disabled: bool) -> void:
	tile.disabled = set_disabled
	tile.modulate = Color(0.7, 0.7, 0.7) if set_disabled else Color.WHITE
	tile.focus_mode = Control.FOCUS_NONE if set_disabled else Control.FOCUS_CLICK


func get_direction(index: int, enemy_tile: Tile) -> int:
	for i: int in directions.size():
		if tile_buttons[index + directions[i]] == enemy_tile:
			return i
	return -1


# Fence Buttons ----------------------------------------------------------------
## Set the fence container size and instance the fence buttons under the 
## container
func instance_fence_buttons(fence_size: int) -> void:
	var fence_button_resource: Resource  = Resources.get_resource('fence_button')
	var total_fences: int = fence_size * fence_size
	var board_size: int = fence_size + 1
	fence_button_container.columns = fence_size
	
	for i: int in range(fence_size * fence_size):
		var fence_button: Button = fence_button_resource.instantiate()
		fence_buttons.append(fence_button)
		fence_button_container.add_child(fence_button, true)
	
	for index: int in range(total_fences):
		var curr_fence: FenceButton = fence_buttons[index]
		curr_fence.set_fence_connections(index, fence_size, fence_buttons)
		curr_fence.set_tile_connections(index, board_size, tile_buttons)


func update_fence_buttons() -> void:
	for fence_button: FenceButton in fence_buttons:
		fence_button.disabled = fence_button.dir_is_disabled[Global.dir_index]
		# Disable mouse filter if the button is disabled
		fence_button.mouse_filter = Control.MOUSE_FILTER_IGNORE if fence_button.disabled else Control.MOUSE_FILTER_STOP
	# TODO Implement illegal fence check


func set_fence_buttons(color: Color) -> void:
	fence_button_container.get_children().map(func(x: FenceButton): x.self_modulate = color)


func confirm_place_fence() -> void:
	# Flip the index (for NESW adjustment)
	var flipped_index: int = 1 - Global.dir_index
	# Get the adjacent directionals
	var disabled_indexes: Array[int] = [flipped_index, flipped_index + 2]
	
	# Disable the adjacents buttons, for that direction
	for indexes: int in disabled_indexes:
		if selected_fence_button.connections[indexes]:
			selected_fence_button.connections[indexes].dir_is_disabled[Global.dir_index] = true
	
	# Loop through the connections in the directed fence index
	for connection: Array in selected_fence_button.tile_connections[Global.dir_index]:
		for index: int in connection.size():
			remove_tile_connection(connection, index)
	
	selected_fence_button.fence_placed = true
	selected_fence_button = null


func remove_tile_connection(connection: Array, index) -> void:
	var tile: Tile = connection[index]
	var inverted_index: int = 1 - index
	var tile_to_remove: Tile = connection[inverted_index]
	var replace_index: int =  tile.connections.find(tile_to_remove)
	tile.connections[replace_index] = null


# Pawns ------------------------------------------------------------------------
@warning_ignore("integer_division")
@warning_ignore("narrowing_conversion")
func spawn_pawns(board_size: int) -> void:
	# For each tile, instance a pawn for both players
	for index: int in range(tile_buttons.size()):
		tile_buttons[index].pawns[0] = spawn_pawn(Global.players[0]["name"], Global.players[0]["color"], index)
		tile_buttons[index].pawns[1] = spawn_pawn(Global.players[1]["name"], Global.players[1]["color"], index)
	
	current_tiles[0] = tile_buttons[board_size * (board_size - 0.5)]
	current_tiles[1] = tile_buttons[board_size / 2]
	
	# Show both pawns
	current_tiles[0].pawns[0].show()
	current_tiles[1].pawns[1].show()


func spawn_pawn(p_name: String, color: Color, pos_index: int) -> Pawn:
	var pawn: Pawn = Resources.get_resource("pawn").instantiate()
	pawn.colour = color
	pawn.player_name = p_name
	tile_buttons[pos_index].add_child(pawn, true)
	pawn.current = tile_buttons[pos_index]
	pawn.hide()
	return pawn


func confirm_move_pawn() -> void:
	# Move the pawn
	current_tiles[current_player].pawns[current_player].hide()
	selected_pawn_tile.pawns[current_player].modulate.a = 1
	
	# Update Pawn
	current_tiles[current_player] = selected_pawn_tile
	
	# Reset selected pawn, enabled pawn moved so the new pawn isn't hidden
	selected_pawn_tile.pawn_moved = true 
	selected_pawn_tile = null


# Signals ----------------------------------------------------------------------
func _on_fence_button_pressed(fence_button: FenceButton) -> void:
	selected_fence_button = fence_button
	selected_pawn_tile = null
	fence_button.h_fence.visible = Global.dir_index == 0
	fence_button.v_fence.visible = Global.dir_index != 0


## Flip the rotation of the fence
func _on_directional_button_pressed() -> void:
	selected_fence_button = null
	Global.dir_index = 1 - Global.dir_index
	toggle_direction_button.text = 'Horizontal' if Global.dir_index == 0 else 'Vertical'
	update_fence_buttons()


func _on_move_pawn_pressed() -> void:
	set_non_adjacent_tiles(current_tiles[current_player], true)
	set_fence_buttons(Color.TRANSPARENT)
	selected_fence_button = null
	last_choice = _on_move_pawn_pressed


func _on_place_fence_pressed() -> void:
	set_non_adjacent_tiles(current_tiles[current_player], false)
	reset_board(Color.WHITE)
	selected_pawn_tile = null
	last_choice = _on_place_fence_pressed


func _on_confirm_pressed() -> void:
	# Reset Board
	reset_board(Color.TRANSPARENT)
	
	if selected_fence_button:
		confirm_place_fence()
	elif selected_pawn_tile:
		confirm_move_pawn()
		
	update_fence_buttons()
	
	# Switch to next player
	current_player = 1 - current_player
