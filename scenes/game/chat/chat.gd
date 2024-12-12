extends Panel

@export var chat_container: VBoxContainer

func add_message(msg: String, player: int) -> void:
	var new_comment: Label = Resources.get_resource("comment").instantiate()
	new_comment.text = msg
	new_comment.modulate = Global.players[player]['color']
	chat_container.add_child(new_comment, true)
