class_name SingleplayerGame extends BaseGame

@export var bot_cover: Control
@export var mini_max: MiniMaxAlgorithm


func _ready() -> void:
	super._ready()
	bot_cover.hide()


func set_current_player(val: int) -> void:
	super.set_current_player(val)
	bot_cover.visible = val == 1

	# Bot's Turn
	if val == 1:
		await RenderingServer.frame_post_draw
		move_code = mini_max.GetBestMove(board, current_player)
		_on_confirm_pressed()


func confirm_place_fence(fence: int) -> void:
	var direction: int = 1 if move_code.substr(2).to_int() < 0 else 0
	_on_fence_button_pressed(abs(fence), direction)
	super.confirm_place_fence(fence)


func confirm_move_pawn(tile: int) -> void:
	_on_tile_pressed(tile)
	super.confirm_move_pawn(tile)


# Undo Twice so that Bot doesn't repeat
func _on_undo_button_pressed() -> void:
	undo_board_ui()
	undo_board_ui()

	finish_undo_board()

	# Force reset turn
	current_player = current_player
