class_name UserInterface extends Control

@export_category("Actions")
@export var toggle_direction_button: Button
@export var confirm_button: Button
@export var undo_button: Button
@export var reset_button: Button

@export_category("Other")
@export var turn_label: Label
@export var chat: Panel
@export var fence_count_labels: Array[Label]

@export_category("Win Screen")
@export var win_label: Label
@export var exit_button: Button

@export_category("Chat")
@export var chat_container: VBoxContainer

var is_bots: bool = false


func _input(event: InputEvent) -> void:
	if event.is_action_pressed("switch_direction"):
		SignalManager.direction_toggled.emit()
	if is_bots:
		return
	if event.is_action_pressed("confirm") and !confirm_button.disabled:
		SignalManager.confirm_pressed.emit()


func _ready() -> void:
	exit_button.pressed.connect(SignalManager.exit_pressed.emit)
	toggle_direction_button.pressed.connect(SignalManager.direction_toggled.emit)
	confirm_button.pressed.connect(SignalManager.confirm_pressed.emit)
	undo_button.pressed.connect(SignalManager.undo_pressed.emit)
	reset_button.pressed.connect(_on_reset_button_pressed)
	undo_button.disabled = true


func update_win(player: int) -> void:
	win_label.text = Global.players[player]["name"] + " Wins!"


func update_fence_counts(player: int, amount: int) -> void:
	fence_count_labels[player].text = str(amount)


func update_turn(player: int) -> void:
	turn_label.text = str(Global.players[player]["name"]) + "'s Turn"


func add_message(msg: String, player: int) -> void:
	var new_comment: Label = Resources.get_resource("comment").instantiate()
	new_comment.text = msg
	new_comment.modulate = Global.players[player]["color"]
	chat_container.add_child(new_comment, true)


## Disable the confirm button when neither option is selected
func set_confirm_button(move_code: String) -> void:
	confirm_button.disabled = move_code == ""


#region Signals
#endregion


func update_direction() -> void:
	toggle_direction_button.text = "Horizontal" if Global.fence_direction == 0 else "Vertical"


func _on_reset_button_pressed() -> void:
	SignalManager.reset_board_requested.emit()
	chat_container.get_children().map(func(x: Control): x.queue_free())
	win_label.text = "Game In Progress"
	undo_button.disabled = true
	confirm_button.disabled = true
