extends Panel

@export var chat_container: VBoxContainer

func add_message(msg: String) -> void:
	var new_comment: Label = Resources.get_resource("comment").instantiate()
	new_comment.text = msg
	chat_container.add_child(new_comment, true)
