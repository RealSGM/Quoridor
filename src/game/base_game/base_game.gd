class_name BaseGame extends Control
## Handles the Interface for the BaseGame
## Sends signals to the board and user interface

@export_category("Nodes")
@export var board: BoardState
@export var tile_container: GridContainer
@export var board_container: PanelContainer
@export var fence_button_container: GridContainer
@export var user_interface: UserInterface
@export var illegal_fence_check: IllegalFenceCheck

var tile_buttons: Array[TileButton] = []
var fence_buttons: Array[FenceButton] = []

## Update board when the player is changed
@onready var current_player: int:
	set(val):
		current_player = val
		set_current_player(val)

@onready var move_code: String = "":
	set(val):
		if not move_code.is_empty():
			var index: int = abs(move_code.split("_")[0].substr(2).to_int())
			match move_code[1]:
				# Clear current fence
				"f":
					if !board.IsFencePlaced(index):
						fence_buttons[index].clear_fences()
				# Clear current tile
				"m":
					tile_buttons[index].clear_pawns()
		move_code = val
		user_interface.set_confirm_button(val)


func _ready() -> void:
	SignalManager.confirm_pressed.connect(_on_confirm_pressed)
	SignalManager.direction_toggled.connect(_on_directional_button_pressed)
	SignalManager.undo_pressed.connect(_on_undo_button_pressed)
	update_fence_direction()
	current_player = 0
	board.show()


func set_current_player(val: int) -> void:
	reset_board()
	set_tiles(board.GetPawnPosition(current_player))
	update_fence_buttons()
	user_interface.update_turn(val)
	board.PrintAllMoves()


## Disable all tiles, and reset their modulate
func reset_board() -> void:
	tile_buttons.map(
		func(tile: TileButton):
			tile.disabled = true
			tile.modulate = Color.WHITE
			tile.focus_mode = Control.FOCUS_NONE
	)


## Setup the board with the selected size
func setup_board() -> void:
	board.InitialiseBoard()

	# Update both players fence counts in UI
	for player: int in Global.BITS:
		user_interface.update_fence_counts(player, Global.MAX_FENCES - board.GetFenceCount(player))

	instance_tile_buttons()
	instance_fence_buttons()
	spawn_pawns()
	reset_board()


## Set the fence container size and instance the fence buttons under the
## container
func instance_fence_buttons() -> void:
	var fence_size: int = Global.BOARD_SIZE - 1
	var fence_button_resource: Resource = Resources.get_resource("fence_button")
	var total_fences: int = fence_size * fence_size
	fence_button_container.columns = fence_size

	for i: int in range(total_fences):
		var fence_button: FenceButton = fence_button_resource.instantiate()
		fence_button.id = i
		fence_button_container.add_child(fence_button, true)
		fence_buttons.append(fence_button)


## Set the grid container size and instance the tiles under the grid
func instance_tile_buttons() -> void:
	var tile_resource: Resource = Resources.get_resource("tile_button")
	tile_container.columns = Global.BOARD_SIZE

	# Instance board state tiles
	for i: int in range(Global.BOARD_SIZE * Global.BOARD_SIZE):
		var tile_button: TileButton = tile_resource.instantiate()
		tile_button.id = i
		tile_container.add_child(tile_button, true)
		tile_buttons.append(tile_button)


#region Fences
#endregion


func update_fence_buttons() -> void:
	for fence: int in range((Global.BOARD_SIZE - 1) * (Global.BOARD_SIZE - 1)):
		var fence_button: FenceButton = fence_buttons[fence]
		fence_button.disabled = not board.GetFenceEnabled(fence, Global.fence_direction) if board.GetFenceCount(current_player) < Global.MAX_FENCES else true
		# Disable mouse filter if the button is disabled
		fence_button.mouse_filter = Control.MOUSE_FILTER_IGNORE if fence_button.disabled else Control.MOUSE_FILTER_STOP


func confirm_place_fence(fence: int, direction: int) -> void:
	user_interface.add_message("Place: " + Helper.GetMoveCodeAsString(current_player, "f", direction, abs(fence), -1), current_player)
	user_interface.update_fence_counts(current_player, Global.MAX_FENCES - board.GetFenceCount(current_player) - 1)


func update_fence_direction() -> void:
	move_code = ""
	user_interface.update_direction()
	update_fence_buttons()


#region Tiles
#endregion


## Set the tiles of the board, based off the current player's turn
func set_tiles(tile: int) -> void:
	tile_buttons[tile].disabled = true
	var enabled_tiles: Array = Array(board.GetReachableTiles(current_player))

	for index: int in range(tile_buttons.size()):
		set_tile_button(tile_buttons[index], index not in enabled_tiles)


## Disable and darken a tile
func set_tile_button(tile: TileButton, is_disabled: bool) -> void:
	tile.disabled = is_disabled
	tile.modulate = Color(0.7, 0.7, 0.7) if is_disabled else Color.WHITE
	tile.focus_mode = Control.FOCUS_NONE if is_disabled else Control.FOCUS_CLICK


#region Pawns
#endregion


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

	# Update UI
	user_interface.add_message("Shift: " + Helper.GetMoveCodeAsString(current_player, "m", 0, tile, current_position), current_player)


#region Signals
#endregion


## Flip the rotation of the fence
func _on_directional_button_pressed() -> void:
	Global.fence_direction = 1 - Global.fence_direction
	update_fence_direction()


func _on_fence_button_pressed(fence: int, direction: int = Global.fence_direction) -> void:
	move_code = Helper.GetMoveCodeAsString(current_player, "f", direction, fence, -1)
	var fence_button: FenceButton = fence_buttons[fence]
	fence_button.h_fence.visible = direction == 0
	fence_button.v_fence.visible = direction != 0

	if not Global.coloured_fences:
		return

	fence_button.h_fence.modulate = Global.players[current_player]["color"]
	fence_button.v_fence.modulate = Global.players[current_player]["color"]


# Format: 0a1_2
# 0: Player, 
# a: Move Type, 
# 1: Tile To, 
# 2: Tile From, 
func _on_tile_pressed(tile: int) -> void:
	move_code = Helper.GetMoveCodeAsString(current_player, "m", 0, tile, board.GetPawnPosition(current_player))
	var pawn: Panel = tile_buttons[tile].pawns[current_player]
	pawn.modulate.a = 0.5
	pawn.show()


func _on_confirm_pressed() -> void:
	reset_board()
	user_interface.confirm_button.disabled = true
	user_interface.undo_button.disabled = false
	var index: int = move_code.split("_")[0].substr(2).to_int()
	var direction: int = 1 if index < 0 else 0
	index = abs(index)

	match move_code[1]:
		"f":
			confirm_place_fence(index, direction)
		"m":
			confirm_move_pawn(index)

	board.AddMove(move_code)
	move_code = ""

	# Check if the pawn has reached end goal
	if board.IsWinner(current_player):
		user_interface.update_win(current_player)
	# Switch to next player
	else:
		# Complete IFS before switching player
		illegal_fence_check.GetIllegalFences(board, 1- current_player)
		current_player = 1 - current_player


func _on_undo_button_pressed() -> void:
	undo_board_ui()
	finish_undo_board()

	current_player = 1 - current_player


func undo_board_ui() -> void:
	var last_move: String = board.UndoMove()
	var player: int = abs(last_move[0].to_int())

	match last_move[1]:
		"f":
			fence_buttons[abs(last_move.substr(2).to_int())].clear_fences()
			user_interface.fence_count_labels[player].text = str(board.GetFenceCount(player))
			user_interface.add_message("Undo Place: " + last_move, 2)
		"m":
			var moves_filtered: String = last_move.split("m")[1]
			var moves: Array = moves_filtered.split("_")
			tile_buttons[moves[0].to_int()].pawns[abs(last_move[0].to_int())].hide()
			tile_buttons[moves[1].to_int()].pawns[abs(last_move[0].to_int())].show()
			user_interface.add_message("Undo Shift: " + last_move, 2)


func finish_undo_board() -> void:
	for player: int in Global.BITS:
		user_interface.update_fence_counts(player, Global.MAX_FENCES - board.GetFenceCount(player))

	illegal_fence_check.GetIllegalFences(board, current_player)
	move_code = ""

	if board.GetMoveHistory().is_empty():
		user_interface.undo_button.disabled = true
