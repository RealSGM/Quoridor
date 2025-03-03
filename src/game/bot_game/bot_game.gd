class_name BotGame extends BaseGame

@export var next_move_button: Button
@export var debug_minimax_button: Button
@export var autoplay_button: Button

var turn_ready: bool = false

func _ready() -> void:
	super._ready()
	user_interface.is_bots = true
	turn_ready = true
	_on_print_minimax_toggled(false)


func set_current_player(val: int) -> void:
	super.set_current_player(val)

	await RenderingServer.frame_post_draw

	if not autoplay_button.is_pressed():
		return

	turn_ready = false
	move_code = MiniMaxAlgorithm.GetBestMove(board, current_player, debug_minimax_button.is_pressed())
	_on_confirm_pressed()
	turn_ready = true



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


func _on_next_move_pressed() -> void:
	if not turn_ready:
		return
	move_code = MiniMaxAlgorithm.GetBestMove(board, current_player, debug_minimax_button.is_pressed())
	_on_confirm_pressed()


func _on_print_minimax_toggled(toggled_on: bool) -> void:
	debug_minimax_button.text = "Debug: %s" % ["On" if toggled_on else "Off"]


func _on_autoplay_button_toggled(toggled_on: bool) -> void:
	autoplay_button.text = "Auto: %s" % ["On" if toggled_on else "Off"]
	next_move_button.disabled = toggled_on

	if toggled_on:
		_on_next_move_pressed()
