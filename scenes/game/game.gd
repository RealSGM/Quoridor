class_name BaseGame extends Control
## Handles the Interface for the BaseGame
## Sends signals to the board

# TODO Minimax Algorithm

@export_category("Board")
@export var board: BoardState
@export var tile_container: GridContainer
@export var board_container: PanelContainer
@export var fence_button_container: GridContainer
@export var fence_amount: int = 10
@export var player_amount: int = 2

@export_category("User Interface")
@export var user_interface: UserInterface
#@export var toggle_direction_button: Button
#@export var confirm_button: Button
#@export var turn_label: Label
#@export var pause_menu: Panel
#@export var pause_exit: Button
#@export var pause_return: Button
#@export var chat: Panel
#@export var fence_count_labels: Array[Label]

#@export_category("Win Screen")
#@export var win_menu: Control
#@export var win_label: Label
#@export var win_exit_button: Button

var threads: Array[Thread] = []
var tile_buttons: Array[TileButton] = []
var fence_buttons: Array[FenceButton] = []
var has_won: bool = false
var move_history: String = ''

## Update the selected fence, and the confirm button
@onready var selected_fence_index: int = -1:
	set(val):
		# Clear current fence
		if selected_fence_index > -1:
			fence_buttons[selected_fence_index].clear_fences()
		
		# Validate the confirm button
		selected_fence_index = val
		user_interface.set_confirm_button(val, selected_tile_index)


## Update the selected tile, and the confirm button
@onready var selected_tile_index: int = -1:
	set(val):
		# Clear the current pawn
		if selected_tile_index > -1:
			tile_buttons[selected_tile_index].clear_pawns()
		
		# Validate the confirm button
		selected_tile_index = val
		user_interface.set_confirm_button(selected_fence_index, val)
		
		if val > -1:
			var pawn: Panel = tile_buttons[selected_tile_index].pawns[current_player]
			pawn.modulate.a = 0.5
			pawn.show()


## Update board when the player is changed
@onready var current_player: int:
	set(val):
		current_player = val
		board.CurrentPlayer = val
		reset_board()
		set_tiles(board.PawnPositions[current_player])
		get_illegal_fences()
		update_fence_buttons()
		user_interface.update_turn(val)
		board.CurrentPlayer = val


func _ready() -> void:
	SignalManager.confirm_pressed.connect(_on_confirm_pressed)
	SignalManager.direction_toggled.connect(_on_directional_button_pressed)
	user_interface.set_confirm_button(selected_fence_index, selected_tile_index)
	current_player = 0
	board.show()
	
	


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
	board.SetAdjacentOffsets(board_size)
	board.GenerateTiles(board_size)
	board.GenerateFences(board_size - 1)
	board.SetFenceCounts(fence_amount, player_amount)
	board.SetPawnPositions(player_amount, board_size)
	board.SetWinPositions(board_size, player_amount)
	
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
		var fence_button: Button = fence_button_resource.instantiate()
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
	for fence_button: FenceButton in fence_buttons:
		fence_button.disabled = fence_button.get_enabled() if board.IsFenceAvailable(current_player) else true
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
		var adj_fences: Array = Array(board.GetConnections(fence, Global.board_size-1))
		var index: int = adj_fences[indexes]
		if index > -1:
			fence_buttons[index].dir_disabled[Global.fence_direction] = true
	
	# Place the fence on the board
	board.PlaceFence(selected_fence_index, Global.fence_direction, current_player)
	
	# Update the UI
	user_interface.update_fence(current_player, selected_fence_index, board.FenceCounts[current_player])
	
	# Update the fence button
	fence_button.fence_placed = true
	fence_button.disabled = true
	selected_fence_index = -1


#endregion

#region Tiles
## Set the tiles of the board, based off the current player's turn
func set_tiles(pawn_index: int) -> void:
	tile_buttons[pawn_index].disabled = true
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


func spawn_pawn(color: Color, index: int) -> Panel:
	var pawn: Panel = Resources.get_resource("pawn").instantiate()
	pawn.modulate = color
	tile_buttons[index].add_child(pawn, true)
	pawn.hide()
	return pawn


func confirm_move_pawn() -> void:
	# Hide current pawn
	var current_position: int = board.PawnPositions[current_player]
	tile_buttons[current_position].pawns[current_player].hide()
	
	# Set the modulate of the selected pawn to one
	var tile_button: TileButton = tile_buttons[selected_tile_index]
	tile_button.pawns[current_player].modulate.a = 1
	tile_button.pawn_moved = true 
	
	# Reset selected pawn, enabled pawn moved so the new pawn isn't hidden
	has_won = board.MovePawn(selected_tile_index)
	
	# Update the UI
	user_interface.update_move(current_player, selected_tile_index)
	
	selected_tile_index = -1


#endregion

#region Illegal Fence Check
func get_illegal_fences() -> void:
	# Stop if current player has no more fences
	if board.FenceCounts[current_player] <= 0:
		return
	
	var illegal_fences: Dictionary = { 0: {}, 1: {} }
	var bits: Array[int] = [0, 1]
	
	# Check each fence button, to see if it is possible
	for fence_button: FenceButton in fence_buttons:
		
		# Reset DFS Array
		fence_button.dfs_disabled = [false, false]
		
		# Ignore any placed fences
		if fence_button.fence_placed:
			continue

		# Loop for both, horizontal and vertical fences
		for fence_dir: int in bits:
			# Ignore fences adjacent to placed fences
			if fence_button.dir_disabled[fence_dir]:
				continue
				
			# Loop for each player
			for player: int in bits:
				var thread: Thread = Thread.new()
				threads.append(thread)
				# Replace 0 with player
				thread.start(_illegal_fence_check_threaded.bind(fence_button.id, fence_dir, player))
	
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
			fence_buttons[fence].dfs_disabled[direction] = true


func _illegal_fence_check_threaded(fence: int, fence_dir: int, player: int) -> Array:
	if board.CheckIllegalFence(fence, fence_dir, player):
		return []
	fence_buttons[fence].dfs_disabled[fence_dir] = true
	return [fence_dir, fence]


#endregion


func _on_fence_button_pressed(fence: int) -> void:
	selected_fence_index = fence
	selected_tile_index = -1

	var fence_button: FenceButton = fence_buttons[fence]
	fence_button.h_fence.visible = Global.fence_direction == 0
	fence_button.v_fence.visible = Global.fence_direction != 0


func _on_tile_pressed(tile: int) -> void:
	selected_tile_index = tile
	selected_fence_index = -1


## Flip the rotation of the fence
func _on_directional_button_pressed() -> void:
	selected_fence_index = -1
	Global.fence_direction = 1 - Global.fence_direction
	update_fence_buttons()


func _on_confirm_pressed() -> void:
	# Reset Board
	reset_board()
	
	if selected_fence_index > -1:
		confirm_place_fence(selected_fence_index)
	elif selected_tile_index > -1:
		confirm_move_pawn()
	
	# Check if the pawn has reached end goal
	if has_won:
		user_interface.update_win(current_player)
	# Switch to next player
	else:
		current_player = 1 - current_player
