class_name SingleplayerGame extends BaseGame

@export var bot_cover: Control

var mini_max: MiniMax

func _ready() -> void:
	super._ready()
	bot_cover.hide()
	mini_max = MiniMax.new()


func set_current_player(val: int) -> void:
	super.set_current_player(val)
	bot_cover.visible = val == 1
	
	# Bot's Turn
	if val == 1:
		mini_max.CreateGameTree(board)
