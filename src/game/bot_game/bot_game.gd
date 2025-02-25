class_name BotGame extends BaseGame

@export var pause_button: Button

var bots_enabled: bool = true
var turn_ready: bool = true

func _ready() -> void:
	super._ready()
	user_interface.is_bots = true


func set_current_player(val: int) -> void:
	super.set_current_player(val)

	await RenderingServer.frame_post_draw


	if not bots_enabled:
		return

	turn_ready = false
	move_code = MiniMaxAlgorithm.GetBestMove(board, current_player)
	_on_confirm_pressed()
	turn_ready = true


func confirm_place_fence(fence: int, direction: int) -> void:
	_on_fence_button_pressed(abs(fence), direction)
	super.confirm_place_fence(fence, direction)


func confirm_move_pawn(tile: int) -> void:
	_on_tile_pressed(tile)
	super.confirm_move_pawn(tile)


func _on_pause_button_pressed() -> void:
	bots_enabled = not bots_enabled
	pause_button.text = "%s Bots" % ["Pause" if bots_enabled else "Unpause"]

	if turn_ready:
		set_current_player(current_player)


func _on_undo_button_pressed() -> void:
	undo_board_ui()
	undo_board_ui()

	finish_undo_board()

	# Force reset turn
	current_player = current_player
