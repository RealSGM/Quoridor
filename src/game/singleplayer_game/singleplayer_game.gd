class_name SingleplayerGame extends BaseGame

@export var bot_cover: Control


func _ready() -> void:
	super._ready()
	bot_cover.hide()
	SignalManager.move_selected.connect(_on_move_selected)
	AlgorithmManager.start_new_game()


func set_current_player(val: int) -> void:
	super.set_current_player(val)
	bot_cover.visible = val == 1

	# Bot's Turn
	if val == 1:
		await RenderingServer.frame_post_draw
		var turns_played: int = move_history.split(";").size()
		AlgorithmManager.minimax.SetMaxDepth(turns_played)
		AlgorithmManager.run(board_wrapper, current_player)


func confirm_place_fence(fence: int, direction: int) -> void:
	_on_fence_button_pressed(abs(fence), direction)
	super.confirm_place_fence(fence, direction)


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


func _on_move_selected(code: String) -> void:
	move_code = code
	_on_confirm_pressed()


func reset_board() -> void:
	super.reset_board()
	AlgorithmManager.start_new_game()
