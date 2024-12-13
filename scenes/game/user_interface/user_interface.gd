class_name UserInterface extends Control

@export_category("Actions")
@export var toggle_direction_button: Button
@export var confirm_button: Button

@export var turn_label: Label
@export var pause_menu: Panel
@export var pause_exit: Button
@export var pause_return: Button
@export var chat: Panel
@export var fence_count_labels: Array[Label]

@export_category("Win Screen")
@export var win_menu: Control
@export var win_label: Label
@export var win_exit_button: Button

@export_category("Chat")
@export var chat_container: VBoxContainer


func _input(event: InputEvent) -> void:
	if event.is_action_pressed("confirm") and !confirm_button.disabled:
		SignalManager.confirm_pressed.emit()
	if event.is_action_pressed("pause"):
		pause_menu.visible = !pause_menu.visible


func _ready() -> void:
	win_exit_button.pressed.connect(SignalManager.exit_pressed.emit)
	pause_exit.pressed.connect(SignalManager.exit_pressed.emit)
	pause_return.pressed.connect(func(): pause_menu.hide())
	toggle_direction_button.pressed.connect(SignalManager.direction_toggled.emit)
	confirm_button.pressed.connect(SignalManager.confirm_pressed.emit)
	
	pause_menu.hide()
	win_menu.hide()


func update_win(player: int) -> void:
	win_label.text = Global.players[player]["name"] + " Wins!"
	win_menu.show()


## Add move to the chat
func update_fence(player: int, fence: int, count: int) -> String:
	add_message("Add Fence: " + str(fence), player)
	fence_count_labels[player].text = str(count)
	return "%sf%s" % [player, fence]


func update_move(player: int, tile: int) -> String:
	add_message("Move Pawn: " + str(tile), player)
	return "%sm%s" % [player, tile]


func update_turn(player: int) -> void:
	turn_label.text = str(Global.players[player]["name"]) + "'s Turn"


func add_message(msg: String, player: int) -> void:
	var new_comment: Label = Resources.get_resource("comment").instantiate()
	new_comment.text = msg
	new_comment.modulate = Global.players[player]['color']
	chat_container.add_child(new_comment, true)


## Disable the confirm button when neither option is selected
func set_confirm_button(fence: int, tile: int) -> void:
	confirm_button.disabled = !(fence >= 0 || tile >= 0)


#region Signals
#endregion

func update_direction() -> void:
	toggle_direction_button.text = 'Horizontal' if Global.fence_direction == 0 else 'Vertical'
