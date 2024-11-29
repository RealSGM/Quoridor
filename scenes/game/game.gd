class_name Game extends Control

'''
	1. Generate all tiles on the board
	2. Add pawn to each tile
	3. Generate all fence buttons

	4. Generate game states
'''


@export_category("Board")
@export var board: BoardState
@export var tile_container: GridContainer
@export var board_container: PanelContainer
@export var fence_button_container: GridContainer

@export_category("User Interface")
@export var toggle_direction_button: Button
@export var confirm_button: Button
@export var exit_button: Button
@export var turn_label: Label

@export_category("Win Screen")
@export var win_cover: Control
@export var win_label: Label
@export var win_exit_button: Button


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
@onready var selected_pawn_tile: TileButton = null:
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

## Update board when the player is changed
@onready var current_player: int:
	set(val):
		current_player = val
		reset_board()
		set_tiles(board.pawn_indexes[current_player], true)
		turn_label.text = str(Global.players[current_player]["name"]) + "'s Turn"


func _ready() -> void:
	_on_directional_button_pressed()
	current_player = 0
	exit_button.pressed.connect(SignalManager.exit_pressed.emit)
	win_exit_button.pressed.connect(SignalManager.exit_pressed.emit)
	
	board.show()
	win_cover.hide()
	exit_button.show()


## Setup the board with the selected size
func setup_board(board_size: int) -> void:
	Global.board_size = board_size
	board.directions = [-board_size, 1, board_size, 1]
	instance_tile_buttons(board_size)
	instance_fence_buttons(board_size - 1)
	spawn_pawns(board_size)
	reset_board()


## Disable the confirm button when neither option is selected
func set_confirm_button(fence_button: FenceButton, tile_button: TileButton) -> void:
	confirm_button.disabled = !(fence_button || tile_button)


## Disable all tiles, and reset their modulate
func reset_board() -> void:
	board.tiles.map(func(tile: Tile): 
		tile.button.disabled = true
		tile.button.modulate = Color.WHITE
	)


func check_win(tile: Tile, bounds: Array) -> bool:
	return board.tiles.find(tile) > bounds[0] and board.tiles.find(tile) < bounds[1]


# Tiles ------------------------------------------------------------------------
## Set the grid container size and instance the tiles under the grid
func instance_tile_buttons(board_size: int) -> void:
	var tile_resource: Resource  = Resources.get_resource('tile_button')
	var total_tiles: int = board_size * board_size
	tile_container.columns = board_size
	
	# Instance board state tiles
	for i: int in range(total_tiles):
		var tile: Tile = Tile.new()
		board.tiles.append(tile)
		var tile_button: TileButton = tile_resource.instantiate()
		tile.button = tile_button
		tile_button.tile = tile
		tile_container.add_child(tile_button, true)
	
	# Setup the connections for each tile
	for index: int in range(total_tiles):
		var curr_tile: Tile = board.tiles[index]
		curr_tile.set_connections(index, board_size, board.tiles)
		
	board.win_indexes = [[0, board_size - 1], [total_tiles - board_size - 1, total_tiles -1]]


## Set the tiles of the board, based off the current player's turn
func set_tiles(pawn_index: int, set_disabled: bool) -> void:
	var tiles_to_enable: Array[TileButton] = []
	var pawn_tile: Tile = board.tiles[pawn_index]
	
	# Loop through all tiles
	for index: int in range(board.tiles.size()):
		var tile: Tile = board.tiles[index]
		
		# Disable the pawn tile, but do not darken
		if tile == pawn_tile:
			tile.button.disabled = true
			
		# Search the current tiles connections that are not blocked by a fence
		elif tile in pawn_tile.connections:
			tiles_to_enable.append_array(get_adjacent_tiles(pawn_tile, tile, index, set_disabled))
		else:
			disable_tile_button(tile.button, set_disabled)
			
	# Enable all selectable tiles that the pawn can move to
	tiles_to_enable.map(func(tile: TileButton):
		if !tile:
			return
		tile.disabled = false
		tile.modulate = Color.WHITE
	)

## Returns all adjacent tile buttons
func get_adjacent_tiles(pawn_tile: Tile, tile: Tile, tile_index: int, set_disabled: bool) -> Array[TileButton]:
	# Empty tile (not taken by enemy pawn
	if tile_index != board.pawn_indexes[1 - current_player]:
		return [tile.button]
	
	# Current tile is taken by enemy pawn, find adjacent tiles of enemy tile
	## TODO Reimplement
	disable_tile_button(tile.button, set_disabled)
	
	# Get the cardinal direction between the two pawns
	var pawn_index: int = board.tiles.find(pawn_tile)
	var dir_index: int = get_direction(pawn_index, tile)
	
	# Check if the leaped index is within the board boundaries
	if pawn_index + (board.directions[dir_index] * 2) > board.tiles.size():
		return []
	
	return get_leaped_tiles(pawn_index, dir_index, tile)


## Disable and darken a tile
func disable_tile_button(tile: TileButton, set_disabled: bool) -> void:
	tile.disabled = set_disabled
	tile.modulate = Color(0.7, 0.7, 0.7) if set_disabled else Color.WHITE
	tile.focus_mode = Control.FOCUS_NONE if set_disabled else Control.FOCUS_CLICK


func get_direction(index: int, enemy_tile: Tile) -> int:
	for i: int in board.directions.size():
		if board.tiles[index + board.directions[i]] == enemy_tile:
			return i
	return -1


func get_leaped_tiles(player_index: int, dir_index: int, tile: Tile) -> Array[TileButton]:
	var leaped_tile: Tile = board.tiles[player_index + (board.directions[dir_index] * 2)]
		
	# Check if there is no fence blocking it
	if leaped_tile && tile.connections[dir_index] == leaped_tile:
		return [leaped_tile.button]
		
	var tiles: Array[TileButton] = []
	# If there is a fence blocking it, add the adjacent positions of the enemy pawn
	for tile_connection: Tile in tile.connections:
		# Remove the current pawns from being added
		if tile_connection != board.tiles[player_index] && tile_connection:
			tiles.append(tile_connection.button)
	return tiles


# Fence Buttons ----------------------------------------------------------------
## Set the fence container size and instance the fence buttons under the 
## container
func instance_fence_buttons(fence_size: int) -> void:
	var fence_button_resource: Resource  = Resources.get_resource('fence_button')
	var total_fences: int = fence_size * fence_size
	var board_size: int = fence_size + 1
	fence_button_container.columns = fence_size
	
	for i: int in range(total_fences):
		var fence: Fence = Fence.new()
		board.fences.append(fence)
		var fence_button: Button = fence_button_resource.instantiate()
		fence.button = fence_button
		fence_button.fence = fence
		fence_button_container.add_child(fence_button, true)
	
	for index: int in range(total_fences):
		var curr_fence: Fence = board.fences[index]
		curr_fence.set_fence_connections(index, fence_size, board.fences)
		curr_fence.set_tile_connections(index, board_size, board.tiles)


func update_fence_buttons() -> void:
	for fence: Fence in board.fences:
		fence.button.disabled = fence.button.dir_is_disabled[Global.fence_direction] if board.fence_counts[current_player] > 0 else true
		# Disable mouse filter if the button is disabled
		fence.button.mouse_filter = Control.MOUSE_FILTER_IGNORE if fence.button.disabled else Control.MOUSE_FILTER_STOP


func confirm_place_fence() -> void:
	# Flip the index (for NESW adjustment)
	var flipped_index: int = 1 - Global.fence_direction
	# Get the adjacent directionals
	var disabled_indexes: Array[int] = [flipped_index, flipped_index + 2]
	
	# Disable the adjacents buttons, for that direction
	for indexes: int in disabled_indexes:
		if selected_fence_button.fence.adj_fences[indexes]:
			selected_fence_button.fence.adj_fences[indexes].button.dir_is_disabled[Global.fence_direction] = true
	
	# Loop through the connections in the directed fence index
	for connection: Array in selected_fence_button.fence.adj_tiles[Global.fence_direction]:
		for index: int in connection.size():
			remove_tile_connection(connection, index)
	
	selected_fence_button.fence_placed = true
	board.fence_counts[current_player] -= 1


func remove_tile_connection(connection: Array, index) -> void:
	var tile: Tile = connection[index]
	var inverted_index: int = 1 - index
	var tile_to_remove: Tile = connection[inverted_index]
	var replace_index: int =  tile.connections.find(tile_to_remove)
	tile.connections[replace_index] = null


func get_illegal_fences(fence_dir: int) -> void:
	var illegals: Array[FenceButton] = []
	for fence_button: FenceButton in board.fence_buttons:
		if is_fence_legal(fence_button, fence_dir):
			continue
		illegals.append(fence_button)


func is_fence_legal(_fence_button: FenceButton, _fence_dir: int) -> bool:
	var bounds: Array = board.win_indexes[current_player]
	var _goal_tiles: Array[Tile] = board.tiles.slice(bounds[0], bounds[1])
	return true


# Pawns ------------------------------------------------------------------------
@warning_ignore("integer_division")
@warning_ignore("narrowing_conversion")
func spawn_pawns(board_size: int) -> void:
	# For each tile, instance a pawn for both players
	for index: int in range(board.tiles.size()):
		board.tiles[index].button.pawns[0] = spawn_pawn(Global.players[0]["name"], Global.players[0]["color"], index)
		board.tiles[index].button.pawns[1] = spawn_pawn(Global.players[1]["name"], Global.players[1]["color"], index)
	
	board.pawn_indexes[0] = int(board_size * (board_size - 0.5))
	board.pawn_indexes[1] = int(board_size / 2)
	
	# Show both pawns
	board.tiles[board.pawn_indexes[0]].button.pawns[0].show()
	board.tiles[board.pawn_indexes[1]].button.pawns[1].show()


func spawn_pawn(p_name: String, color: Color, pos_index: int) -> Pawn:
	var pawn: Pawn = Resources.get_resource("pawn").instantiate()
	pawn.colour = color
	pawn.player_name = p_name
	board.tiles[pos_index].button.add_child(pawn, true)
	pawn.current = board.tiles[pos_index].button
	pawn.hide()
	return pawn


func confirm_move_pawn() -> void:
	var current_position: int = board.pawn_indexes[current_player]
	
	# Move the pawn
	board.tiles[current_position].button.pawns[current_player].hide()
	selected_pawn_tile.pawns[current_player].modulate.a = 1
	
	# Update pawn
	selected_pawn_tile.pawn_moved = true 
	
	# Reset selected pawn, enabled pawn moved so the new pawn isn't hidden
	board.pawn_indexes[current_player] = board.tiles.find(selected_pawn_tile.tile)


# Signals ----------------------------------------------------------------------
func _on_fence_button_pressed(fence_button: FenceButton) -> void:
	selected_fence_button = fence_button
	selected_pawn_tile = null
	fence_button.h_fence.visible = Global.fence_direction == 0
	fence_button.v_fence.visible = Global.fence_direction != 0


func _on_tile_pressed(tile_button: TileButton) -> void:
	selected_pawn_tile = tile_button
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
		has_won = check_win(selected_pawn_tile.tile, board.win_indexes[current_player])
		selected_pawn_tile = null
	
	# Check if the pawn has reached end goal
	if has_won:
		win_label.text = Global.players[current_player]["name"] + " Wins!"
		win_cover.show()
		exit_button.hide()
	# Switch to next player
	else:
		current_player = 1 - current_player
	
	update_fence_buttons()
