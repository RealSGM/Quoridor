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
		start_minimax()


func start_minimax() -> void:
	# Get all possible fence buttons
	var enabled_fences: Dictionary ={
		0: [],
		1: [],
	}
	
	# Loop through both directions
	#for bit: int in Global.BITS:
		## Loop through all fence buttons
		#for fence_button: FenceButton in fence_buttons:
			## Check if the fence button is enabled, per direction
			#if not(fence_button.dfs_disabled[bit] or fence_button.dir_disabled[bit]):
				#enabled_fences[bit].append(fence_buttons.find(fence_button))
	
	#mini_max.CreateGameTree(enabled_fences)
