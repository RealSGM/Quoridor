class_name BaseGame extends Control
## Handles the Interface for the BaseGame
## Sends signals to the board and user interface

@export_category("Game Settings")
@export var fence_amount: int = 10
@export var player_amount: int = 2

@export_category("Nodes")
@export var board: BoardState
@export var tile_container: GridContainer
@export var board_container: PanelContainer
@export var fence_button_container: GridContainer
@export var user_interface: UserInterface

var tile_buttons: Array[TileButton] = []
var fence_buttons: Array[FenceButton] = []
var has_won: bool = false

## Update the selected fence, and the confirm button
@onready var selected_fence: int = -1:
	set(val):
		# Clear current fence
		if selected_fence > -1:
			fence_buttons[selected_fence].clear_fences(board.GetFencePlaced(selected_fence))
		
		# Validate the confirm button
		selected_fence = val
		user_interface.set_confirm_button(val, selected_tile)


## Update the selected tile, and the confirm button
@onready var selected_tile: int = -1:
	set(val):
		# Clear the current pawn
		if selected_tile > -1:
			tile_buttons[selected_tile].clear_pawns()
		
		# Validate the confirm button
		selected_tile = val
		user_interface.set_confirm_button(selected_fence, val)
		
		if val > -1:
			var pawn: Panel = tile_buttons[selected_tile].pawns[current_player]
			pawn.modulate.a = 0.5
			pawn.show()


## Update board when the player is changed
@onready var current_player: int:
	set(val):
		current_player = val
		board.CurrentPlayer = val
		set_current_player(val)


func _ready() -> void:
	SignalManager.confirm_pressed.connect(_on_confirm_pressed)
	SignalManager.direction_toggled.connect(_on_directional_button_pressed)
	user_interface.set_confirm_button(selected_fence, selected_tile)
	current_player = 0
	board.show()


func set_current_player(val: int) -> void:
	reset_board()
	set_tiles(board.PawnPositions[current_player])
	#get_illegal_fences(board);
	update_fence_buttons()
	user_interface.update_turn(val)


## Disable all tiles, and reset their modulate
func reset_board() -> void:
	tile_buttons.map(func(tile: TileButton):
		tile.disabled = true
		tile.modulate = Color.WHITE
		tile.focus_mode = Control.FOCUS_NONE
	)


## Setup the board with the selected size
func setup_board(board_size: int) -> void:
	Global.board_size = board_size
	board.InitialiseAdjacentOffsets(board_size)
	board.GenerateTiles(board_size)
	board.GenerateFences(board_size - 1)
	board.InitialiseFenceCounts(fence_amount, player_amount)
	board.InitialisePawnPositions(player_amount, board_size)
	board.InitialiseWinPositions(board_size, player_amount)
	
	instance_tile_buttons(board_size)
	instance_fence_buttons(board_size - 1)
	spawn_pawns()
	reset_board()


## Set the fence container size and instance the fence buttons under the 
## container
func instance_fence_buttons(fence_size: int) -> void:
	var fence_button_resource: Resource  = Resources.get_resource('fence_button')
	var total_fences: int = fence_size * fence_size
	fence_button_container.columns = fence_size
	
	for i: int in range(total_fences):
		var fence_button: FenceButton = fence_button_resource.instantiate()
		fence_button.id = i
		fence_button_container.add_child(fence_button, true)
		fence_buttons.append(fence_button)


## Set the grid container size and instance the tiles under the grid
func instance_tile_buttons(board_size: int) -> void:
	var tile_resource: Resource  = Resources.get_resource('tile_button')
	tile_container.columns = board_size
	
	# Instance board state tiles
	for i: int in range(board_size * board_size):
		var tile_button: TileButton = tile_resource.instantiate()
		tile_button.id = i
		tile_container.add_child(tile_button, true)
		tile_buttons.append(tile_button)


#region Fences
func update_fence_buttons() -> void:
	for fence: int in range(board.GetFenceAmount()):
		var fence_button: FenceButton = fence_buttons[fence]
		fence_button.disabled = board.GetFenceEnabled(fence, Global.fence_direction) if board.IsFenceAvailable(current_player) else true
		# Disable mouse filter if the button is disabled
		fence_button.mouse_filter = Control.MOUSE_FILTER_IGNORE if fence_button.disabled else Control.MOUSE_FILTER_STOP


func confirm_place_fence(fence: int) -> void:
	var fence_button: FenceButton = fence_buttons[fence]
	
	# Flip the index (for NESW adjustment)
	var flipped_index: int = 1 - Global.fence_direction
	
	# Get the adjacent directionals
	var disabled_indexes: Array[int] = [flipped_index, flipped_index + 2]
	
	# Disable the adjacents buttons, for that direction
	for indexes: int in disabled_indexes:
		var adj_fences: Array = Array(board.InitialiseConnections(fence, Global.board_size-1))
		var index: int = adj_fences[indexes]
		if index > -1:
			board.SetDirDisabled(index, Global.fence_direction, true)
	
	# Update Board
	board.AddMove("%sf%s" % [current_player, Global.map_fence_direction(fence)])
	
	# Update UI
	user_interface.add_message("Add Fence: " + str(fence), current_player)
	user_interface.fence_count_labels[current_player].text = str(board.FenceCounts[current_player])
	
	fence_button.disabled = true
	selected_fence = -1


#endregion

#region Tiles
## Set the tiles of the board, based off the current player's turn
func set_tiles(tile: int) -> void:
	tile_buttons[tile].disabled = true
	var enabled_tiles: Array = Array(board.GetSelectableTiles(current_player))
	
	for index: int in range(tile_buttons.size()):
		set_tile_button(tile_buttons[index], index not in enabled_tiles)


## Disable and darken a tile
func set_tile_button(tile: TileButton, is_disabled: bool) -> void:
	tile.disabled = is_disabled
	tile.modulate = Color(0.7, 0.7, 0.7) if is_disabled else Color.WHITE
	tile.focus_mode = Control.FOCUS_NONE if is_disabled else Control.FOCUS_CLICK


#endregion


#region Pawns
@warning_ignore("integer_division")
@warning_ignore("narrowing_conversion")
func spawn_pawns() -> void:
	# For each tile, instance a pawn for both players
	for tile_button: TileButton in tile_buttons:
		tile_button.pawns[0] = spawn_pawn(Global.players[0]["color"], tile_button.id)
		tile_button.pawns[1] = spawn_pawn(Global.players[1]["color"], tile_button.id)
	
	tile_buttons[board.PawnPositions[0]].pawns[0].show()
	tile_buttons[board.PawnPositions[1]].pawns[1].show()


func spawn_pawn(color: Color, tile: int) -> Panel:
	var pawn: Panel = Resources.get_resource("pawn").instantiate()
	pawn.modulate = color
	tile_buttons[tile].add_child(pawn, true)
	pawn.hide()
	return pawn


func confirm_move_pawn(tile: int) -> void:
	# Hide current pawn
	var current_position: int = board.PawnPositions[current_player]
	tile_buttons[current_position].pawns[current_player].hide()
	
	# Set the modulate of the selected pawn to one
	var tile_button: TileButton = tile_buttons[tile]
	tile_button.pawns[current_player].modulate.a = 1
	tile_button.pawn_moved = true 
	
	# Update the Board
	board.AddMove("%sm%s" % [current_player, tile])
	
	has_won = board.GetWinner(current_player)
	
	# Update UI
	user_interface.add_message("Move Pawn: " + str(tile), current_player)
	selected_tile = -1
	
	
#endregion


#region Illegal Fence Check

func get_illegal_fences(current_board: BoardState) -> void:
	var threads: Array[Thread] = []
	
	# Stop if current player has no more fences
	if current_board.FenceCounts[board.CurrentPlayer] <= 0:
		return
	
	var illegal_fences: Dictionary = { 0: {}, 1: {} }
	
	# Check each fence button, to see if it is possible
	for fence: int in range(current_board.GetFenceAmount()):
		
		
		# Reset DFS Array
		current_board.SetDFSDisabled(fence, 0, false)
		current_board.SetDFSDisabled(fence, 1, false)
		
		# Ignore any placed fences
		if current_board.GetFencePlaced(fence):
			continue

		# Loop for both, horizontal and vertical fences
		for fence_dir: int in Global.BITS:
			# Ignore fences adjacent to placed fences
			if current_board.GetDirDisabled(fence, fence_dir):
				continue
				
			# Loop for each player
			for player: int in Global.BITS:
				var thread: Thread = Thread.new()
				threads.append(thread)
				thread.start(_illegal_fence_check_threaded.bind(fence, fence_dir, player, board))
	
	# Store results from threads into dictionary
	for thread: Thread in threads:
		var result: Array = thread.wait_to_finish()
		if result.is_empty():
			continue
		illegal_fences[result[0]][result[1]] = true

	threads.clear()
	
	# Set results into fence buttons
	for direction: int in illegal_fences:
		for fence: int in illegal_fences[direction]:
			board.SetDFSDisabled(fence, direction, true)


func _illegal_fence_check_threaded(fence: int, fence_dir: int, player: int, current_board: BoardState) -> Array:
	if current_board.CheckIllegalFence(Global.map_fence_direction(fence), player):
		return []
	current_board.SetDFSDisabled(fence, fence_dir, true)
	return [fence_dir, fence]


#endregion


func _on_fence_button_pressed(fence: int) -> void:
	selected_fence = fence
	selected_tile = -1

	var fence_button: FenceButton = fence_buttons[fence]
	fence_button.h_fence.visible = Global.fence_direction == 0
	fence_button.v_fence.visible = Global.fence_direction != 0


func _on_tile_pressed(tile: int) -> void:
	selected_tile = tile
	selected_fence = -1


## Flip the rotation of the fence
func _on_directional_button_pressed() -> void:
	selected_fence = -1
	Global.fence_direction = 1 - Global.fence_direction
	user_interface.update_direction()
	update_fence_buttons()


func _on_confirm_pressed() -> void:
	# Reset Board
	reset_board()
	
	if selected_fence > -1:
		confirm_place_fence(selected_fence)
	elif selected_tile > -1:
		confirm_move_pawn(selected_tile)
	
	# Check if the pawn has reached end goal
	if has_won:
		user_interface.update_win(current_player)
	# Switch to next player
	else:
		current_player = 1 - current_player
