class_name SingleplayerGame extends BaseGame

@export var bot_cover: Control

func _ready() -> void:
	super._ready()
	bot_cover.hide()


func set_current_player(val: int) -> void:
	super.set_current_player(val)
	bot_cover.visible = val == 1
