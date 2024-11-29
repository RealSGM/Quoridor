class_name Board extends Control

# TODO Illegal Fence Check
# TODO Minimax Algorithm

@export_category("Board")
@export var board_anchor: Control
@export var tile_container: GridContainer
@export var board_container: PanelContainer
@export var fence_button_container: GridContainer
@export var fence_counts: Array[int] = [10, 10]

@export_category("User Interface")
@export var toggle_direction_button: Button
@export var confirm_button: Button
@export var exit_button: Button
@export var turn_label: Label

@export_category("Win Screen")
@export var win_cover: Control
@export var win_label: Label
@export var win_exit_button: Button

## Stored as [Player One Tile, Player Two Tile]
var current_tiles: Array[Tile] = [null, null]
## 1D Array containing every fence button
var fence_buttons: Array[FenceButton] = []
## 1D Array containing every tile
var tile_buttons: Array[Tile] = []
## Stores the adjacent cardinal directions relative to the board
var directions: Array[int] = []
## Stores the upper and lower bounds for each pawn's finish line
var win_indexes: Array[Array] = [[], []]

## Update the selected fence, and the confirm button
@onready var selected_fence_button: FenceButton = null:
	set(val):
		# Clear current fence
		if selected_fence_button:
			selected_fence_button.clear_fences()
		
		# Validate the confirm button
		selected_fence_button = val
		set_confirm_button(val, selected_pawn_tile)

## Update the selected tile, and the confirm button
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

## Update hte
@onready var current_player: int:
	set(val):
		current_player = val
		reset_board()
		set_tiles(current_tiles[current_player], true)
		turn_label.text = str(Global.players[current_player]["name"]) + "'s Turn"


func _ready() -> void:
	_on_directional_button_pressed()
	current_player = 0
	exit_button.pressed.connect(SignalManager.exit_pressed.emit)
	win_exit_button.pressed.connect(SignalManager.exit_pressed.emit)
	SignalManager.tile_pressed.connect(_on_tile_pressed)
	
	board_anchor.show()
	win_cover.hide()
	exit_button.show()


## Setup the board with the selected size
func setup_board(board_size: int) -> void:
	Global.board_size = board_size
	directions = [-Global.board_size, 1, Global.board_size, 1]
	instance_tiles(board_size)
	instance_fence_buttons(board_size - 1)
	spawn_pawns(board_size)
	reset_board()


## Disable the confirm button when neither option is selected
func set_confirm_button(fence_button: FenceButton, tile_button: Tile) -> void:
	confirm_button.disabled = !(fence_button || tile_button)


## Disable all tiles, and reset their modulate
func reset_board() -> void:
	tile_buttons.map(func(tile: Tile): 
		tile.disabled = true
		tile.modulate = Color.WHITE
	)


func check_win(tile: Tile, bounds: Array) -> bool:
	return tile_buttons.find(tile) > bounds[0] and tile_buttons.find(tile) < bounds[1]


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
		
	win_indexes = [[0, board_size - 1], [total_tiles - board_size - 1, total_tiles -1]]


## Set the tiles of the board, based off the current player's turn
func set_tiles(player_tile: Tile, set_disabled: bool) -> void:
	var tiles_to_enable: Array[Tile] = []
	
	for tile: Tile in tile_buttons:
		# Disable the current tile, but do not darken
		if tile == player_tile:
			tile.disabled = true
			
		# Search the current tiles connections that are not blocked by a fence
		elif tile in player_tile.connections:
			tiles_to_enable.append_array(get_adjacent_tiles(player_tile, tile, set_disabled))
		else:
			fully_disable_tile(tile, set_disabled)
	
	# Enable all selectable tiles that the pawn can move to
	tiles_to_enable.map(func(tile: Tile):
		if !tile:
			return
		tile.disabled = false
		tile.modulate = Color.WHITE
	)


func get_adjacent_tiles(player_tile, tile: Tile, set_disabled: bool) -> Array:
	# Empty tile
	if tile != current_tiles[1 - current_player]:
		return [tile]
	
	# Current tile is taken by enemy pawn, find adjacent tiles of enemy tile
	fully_disable_tile(tile, set_disabled)
	
	# Get the tile index of the pawn
	var player_index: int = tile_buttons.find(player_tile)
	# Get the cardinal direction between the two pawns
	var dir_index: int = get_direction(player_index, tile)
	
	# Check if the leaped index is within the board boundaries
	if player_index + (directions[dir_index] * 2) > tile_buttons.size():
		return []
	
	return get_leaped_tiles(player_index, dir_index, tile)


## Disable and darken a tile
func fully_disable_tile(tile: Tile, set_disabled: bool) -> void:
	tile.disabled = set_disabled
	tile.modulate = Color(0.7, 0.7, 0.7) if set_disabled else Color.WHITE
	tile.focus_mode = Control.FOCUS_NONE if set_disabled else Control.FOCUS_CLICK


func get_direction(index: int, enemy_tile: Tile) -> int:
	for i: int in directions.size():
		if tile_buttons[index + directions[i]] == enemy_tile:
			return i
	return -1


func get_leaped_tiles(player_index: int, dir_index: int, tile: Tile) -> Array:
	var leaped_tile: Tile = tile_buttons[player_index + (directions[dir_index] * 2)]
		
	# Check if there is no fence blocking it
	if leaped_tile && tile.connections[dir_index] == leaped_tile:
		return [leaped_tile]
		
	var tiles: Array[Tile] = []
	# If there is a fence blocking it, add the adjacent positions of the enemy pawn
	for tile_connection: Tile in tile.connections:
		# Remove the current pawns from being added
		if tile_connection != current_tiles[current_player]:
			tiles.append(tile_connection)
	
	return tiles


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
		fence_button.disabled = fence_button.dir_is_disabled[Global.fence_direction] if fence_counts[current_player] > 0 else true
		# Disable mouse filter if the button is disabled
		fence_button.mouse_filter = Control.MOUSE_FILTER_IGNORE if fence_button.disabled else Control.MOUSE_FILTER_STOP
	# TODO Implement illegal fence check


func confirm_place_fence() -> void:
	# Flip the index (for NESW adjustment)
	var flipped_index: int = 1 - Global.fence_direction
	# Get the adjacent directionals
	var disabled_indexes: Array[int] = [flipped_index, flipped_index + 2]
	
	# Disable the adjacents buttons, for that direction
	for indexes: int in disabled_indexes:
		if selected_fence_button.connections[indexes]:
			selected_fence_button.connections[indexes].dir_is_disabled[Global.fence_direction] = true
	
	# Loop through the connections in the directed fence index
	for connection: Array in selected_fence_button.tile_connections[Global.fence_direction]:
		for index: int in connection.size():
			remove_tile_connection(connection, index)
	
	selected_fence_button.fence_placed = true
	fence_counts[current_player] -= 1


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


# Signals ----------------------------------------------------------------------
func _on_fence_button_pressed(fence_button: FenceButton) -> void:
	selected_fence_button = fence_button
	selected_pawn_tile = null
	fence_button.h_fence.visible = Global.fence_direction == 0
	fence_button.v_fence.visible = Global.fence_direction != 0


func _on_tile_pressed(tile: Tile) -> void:
	selected_pawn_tile = tile
	selected_fence_button = null


## Flip the rotation of the fence
func _on_directional_button_pressed() -> void:
	selected_fence_button = null
	Global.fence_direction = 1 - Global.fence_direction
	toggle_direction_button.text = 'Horizontal' if Global.fence_direction == 0 else 'Vertical'
	update_fence_buttons()


func _on_confirm_pressed() -> void:
	# Reset Board
	reset_board()
	
	var has_won: bool = false
	
	if selected_fence_button:
		confirm_place_fence()
		selected_fence_button = null
		
	elif selected_pawn_tile:
		confirm_move_pawn()
		has_won = check_win(selected_pawn_tile, win_indexes[current_player])
		selected_pawn_tile = null
	
	update_fence_buttons()
	
	# Check if the pawn has reached end goal
	if has_won:
		win_label.text = Global.players[current_player]["name"] + " Wins!"
		win_cover.show()
		exit_button.hide()
	# Switch to next player
	else:
		current_player = 1 - current_player
