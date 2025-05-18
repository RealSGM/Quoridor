class_name BotGame extends BaseGame

@export var next_move_button: Button
@export var autoplay_button: Button

var turn_ready: bool = false


func _ready() -> void:
	super._ready()
	user_interface.is_bots = true
	turn_ready = true
	SignalManager.move_selected.connect(_on_move_selected)
	AlgorithmManager.start_new_game()


func set_current_player(val: int) -> void:
	super.set_current_player(val)

	await RenderingServer.frame_post_draw

	if not autoplay_button.is_pressed():
		return

	turn_ready = false
	play_turn()
	turn_ready = true


func reset_board() -> void:
	super.reset_board()
	AlgorithmManager.start_new_game()


func confirm_place_fence(fence: int, direction: int) -> void:
	_on_fence_button_pressed(abs(fence), direction)
	super.confirm_place_fence(fence, direction)


func confirm_move_pawn(tile: int) -> void:
	_on_tile_pressed(tile)
	super.confirm_move_pawn(tile)


func _on_undo_button_pressed() -> void:
	undo_board_ui()
	finish_undo_board()
	current_player = 1 - current_player


func update_winner(player: int) -> void:
	var winning_algorithm: Node = AlgorithmManager.chosen_algorithms[player]
	var losing_algorithm: Node = AlgorithmManager.chosen_algorithms[1 - player]
	SignalManager.data_collected.emit(winning_algorithm, "wins", 1)
	SignalManager.data_collected.emit(winning_algorithm, "games_played", 1)
	SignalManager.data_collected.emit(losing_algorithm, "games_played", 1)
	AlgorithmManager.end_game()

	if autoplay_button.is_pressed():
		user_interface._on_reset_button_pressed()


func play_turn() -> void:
	var turns_played: int = move_history.split(";").size()
	AlgorithmManager.minimax.SetMaxDepth(turns_played)
	AlgorithmManager.run(board_wrapper, current_player)


func _on_next_move_pressed() -> void:
	if not turn_ready:
		return
	play_turn()


func _on_autoplay_button_toggled(toggled_on: bool) -> void:
	autoplay_button.text = "Auto: %s" % ["On" if toggled_on else "Off"]
	var game_over: bool = board_wrapper.IsWinner(0) || board_wrapper.IsWinner(1)
	next_move_button.disabled = toggled_on || game_over

	if toggled_on && not game_over:
		_on_next_move_pressed()


func _on_move_selected(code: String) -> void:
	move_code = code
	_on_confirm_pressed()
