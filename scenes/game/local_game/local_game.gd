class_name LocalGame extends BaseGame

func set_current_player(val: int) -> void:
	reset_board()
	set_tiles(board.PawnPositions[current_player])
	get_illegal_fences()
	update_fence_buttons()
	user_interface.update_turn(val)
