class_name BaseGame extends Control
## Handles the Interface for the BaseGame
## Sends signals to the board and user interface

@export_category("Game Settings")
@export var fence_amount: int = 10

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
		board.CurrentPlayer = val
		set_current_player(val)

@onready var move_code: String = "":
	set(val):
		if not move_code.is_empty():
			var index: int = abs(move_code.substr(2).to_int())
			match move_code[1]:
				# Clear current fence
				"f":
					if !board.IsFencePlaced(abs(index)):
						fence_buttons[index].clear_fences()
				# Clear current tile
				"m":
					tile_buttons[index].clear_pawns()
		move_code = val
		user_interface.set_confirm_button(val)


func _ready() -> void:
	SignalManager.confirm_pressed.connect(_on_confirm_pressed)
	SignalManager.direction_toggled.connect(_on_directional_button_pressed)
	_on_directional_button_pressed()
	move_code = ""
	current_player = 0
	board.show()


func set_current_player(val: int) -> void:
	reset_board()
	set_tiles(board.GetPawnPosition(current_player))
	update_fence_buttons()
	user_interface.update_turn(val)


## Disable all tiles, and reset their modulate
func reset_board() -> void:
	tile_buttons.map(
		func(tile: TileButton):
			tile.disabled = true
			tile.modulate = Color.WHITE
			tile.focus_mode = Control.FOCUS_NONE
	)


## Setup the board with the selected size
func setup_board(board_size: int) -> void:
	board.InitialiseBoard(board_size, fence_amount)

	instance_tile_buttons(board_size)
	instance_fence_buttons(board_size - 1)
	spawn_pawns()
	reset_board()


## Set the fence container size and instance the fence buttons under the
## container
func instance_fence_buttons(fence_size: int) -> void:
	var fence_button_resource: Resource = Resources.get_resource("fence_button")
	var total_fences: int = fence_size * fence_size
	fence_button_container.columns = fence_size

	for i: int in range(total_fences):
		var fence_button: FenceButton = fence_button_resource.instantiate()
		fence_button.id = i
		fence_button_container.add_child(fence_button, true)
		fence_buttons.append(fence_button)


## Set the grid container size and instance the tiles under the grid
func instance_tile_buttons(board_size: int) -> void:
	var tile_resource: Resource = Resources.get_resource("tile_button")
	tile_container.columns = board_size

	# Instance board state tiles
	for i: int in range(board_size * board_size):
		var tile_button: TileButton = tile_resource.instantiate()
		tile_button.id = i
		tile_container.add_child(tile_button, true)
		tile_buttons.append(tile_button)


#region Fences
#endregion


func update_fence_buttons() -> void:
	for fence: int in range(board.GetFenceAmount()):
		var fence_button: FenceButton = fence_buttons[fence]
		fence_button.disabled = not board.GetFenceEnabled(fence, Global.fence_direction) if board.GetFenceCount(current_player) > 0 else true
		# Disable mouse filter if the button is disabled
		fence_button.mouse_filter = Control.MOUSE_FILTER_IGNORE if fence_button.disabled else Control.MOUSE_FILTER_STOP


func confirm_place_fence(fence: int) -> void:
	user_interface.add_message("Add Fence: " + str(fence), current_player)
	user_interface.fence_count_labels[current_player].text = str(board.GetFenceCount(current_player) - 1)


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
	user_interface.add_message("Move Pawn: " + str(tile), current_player)


#region Signals
#endregion


## Flip the rotation of the fence
func _on_directional_button_pressed() -> void:
	move_code = ""
	Global.fence_direction = 1 - Global.fence_direction
	user_interface.update_direction()
	update_fence_buttons()


func _on_fence_button_pressed(fence: int, direction: int = Global.fence_direction) -> void:
	move_code = "%sf%s" % [current_player, Helper.GetMappedFenceIndex(fence, direction)]

	var fence_button: FenceButton = fence_buttons[fence]
	fence_button.h_fence.visible = direction == 0
	fence_button.v_fence.visible = direction != 0


func _on_tile_pressed(tile: int) -> void:
	move_code = "%sm%s" % [current_player, tile]

	var pawn: Panel = tile_buttons[tile].pawns[current_player]
	pawn.modulate.a = 0.5
	pawn.show()


func _on_confirm_pressed() -> void:
	# Reset Board
	reset_board()

	var index: int = abs(move_code.substr(2).to_int())

	match move_code[1]:
		"f":
			confirm_place_fence(index)
		"m":
			confirm_move_pawn(index)

	board.AddMove(move_code)
	move_code = ""

	# Check if the pawn has reached end goal
	if board.GetWinner(current_player):
		user_interface.update_win(current_player)
	# Switch to next player
	else:
		# Complete IFS before switching player
		illegal_fence_check.GetIllegalFences(board)
		current_player = 1 - current_player
