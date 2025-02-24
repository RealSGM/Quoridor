class_name BotGame extends BaseGame

func _ready() -> void:
	super._ready()
	user_interface.set_process_input(false)


func set_current_player(val: int) -> void:
	super.set_current_player(val)

	await RenderingServer.frame_post_draw
	move_code = MiniMaxAlgorithm.GetBestMove(board, current_player)
	_on_confirm_pressed()


func confirm_place_fence(fence: int, direction: int) -> void:
	_on_fence_button_pressed(abs(fence), direction)
	super.confirm_place_fence(fence, direction)


func confirm_move_pawn(tile: int) -> void:
	_on_tile_pressed(tile)
	super.confirm_move_pawn(tile)
