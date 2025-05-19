extends Control

@export_category("Menus")
@export var main_menu: Control
@export var play_menu: Control
@export var multiplayer_menu: Control
@export var board_options_menu: Control
@export var background: ColorRect

@export_category("Main Menu Buttons")
@export var play_button: Button
@export var exit_button: Button

@export_category("Play Menu Buttons")
@export var singleplayer_button: Button
@export var multiplayer_button: Button
@export var play_back_button: Button
@export var bot_v_bot_button: Button
@export var qlearning_button: Button

@export_category("Board Options")
@export var board_vbc: VBoxContainer
@export var start_game_button: Button
@export var board_options_back_button: Button

@export_category("Global Options")
@export var board_container: VBoxContainer
@export var player_one_container: VBoxContainer
@export var p_one_colours: OptionButton
@export var player_one_name: LineEdit
@export var fence_coloured_button: Button

@export_category("Local Options")
@export var p_two_colours: OptionButton
@export var player_two_name: LineEdit
@export var player_two_container: VBoxContainer

@export_category("Bot Options")
@export var bot_one_container: VBoxContainer
@export var bot_one_algorithms: OptionButton
@export var bot_one_colours: OptionButton

@export var bot_two_container: VBoxContainer
@export var bot_two_algorithms: OptionButton
@export var bot_two_colours: OptionButton

@export_category("Game Settings")
@export var board_dimensions: float = 800
@export var algorithm_names: Array[String] = ["Minimax", "MCTS", "QLearning", "Random"]

@export var global_options: Array[BoxContainer] = [board_container]
@export var player_containers: Array[VBoxContainer] = [player_one_container, player_two_container, bot_one_container, bot_two_container]
@export var menus: Array[Control] = [main_menu, play_menu, multiplayer_menu, board_options_menu]

var menu_stack: Array = []
var game_type: String

@onready var game_type_dict: Dictionary[String, Array] = {
	"Singleplayer": [player_one_container, bot_two_container],
	"Multiplayer": [player_one_container, player_two_container],
	"BotVBot": [bot_one_container, bot_two_container],
	"QLearning": []
	}

func _input(event: InputEvent) -> void:
	if event.is_action_pressed("toggle_console"):
		Console.visible = !Console.visible


func _ready() -> void:
	setup_menus()
	SignalManager.exit_pressed.connect(_on_exit_button_pressed)


## Setup the menus, ensure all panels are hidden on launch
## Connect all buttons to appropriate signals
func setup_menus() -> void:
	show_main_menu()

	play_button.pressed.connect(_on_menu_button_pressed.bind(play_menu))
	exit_button.pressed.connect(get_tree().quit)

	singleplayer_button.pressed.connect(_on_menu_button_pressed.bind(board_options_menu, singleplayer_button))
	multiplayer_button.pressed.connect(_on_menu_button_pressed.bind(board_options_menu, multiplayer_button))
	bot_v_bot_button.pressed.connect(_on_menu_button_pressed.bind(board_options_menu, bot_v_bot_button))
	qlearning_button.pressed.connect(_on_qlearning_game_pressed)
	play_back_button.pressed.connect(_on_back_button_pressed)

	start_game_button.pressed.connect(_on_start_game_pressed)
	board_options_back_button.pressed.connect(_on_back_button_pressed)

	# Setup colour option buttons
	p_one_colours.item_selected.connect(_on_colour_selected.bind(p_one_colours.selected, p_two_colours))
	p_two_colours.item_selected.connect(_on_colour_selected.bind(p_two_colours.selected, p_one_colours))
	bot_two_colours.item_selected.connect(_on_colour_selected.bind(bot_two_colours.selected, p_one_colours))

	p_one_colours.select(0)
	p_two_colours.select(1)
	bot_one_colours.select(2)
	bot_two_colours.select(3)

	setup_algorithm_names()

	fence_coloured_button.set_pressed(true)


func setup_algorithm_names() -> void:
	for algo: String in algorithm_names:
		bot_one_algorithms.add_item(algo)
		bot_two_algorithms.add_item(algo)

	# Set the option to the latest added algorithm
	bot_one_algorithms.selected = 0
	bot_two_algorithms.selected = 0
	bot_one_algorithms.clear_radio_boxes()
	bot_two_algorithms.clear_radio_boxes()


func show_main_menu() -> void:
	menus.map(func(menu: Control): menu.hide())
	menu_stack.clear()
	main_menu.show()
	menu_stack.append(main_menu)


func set_game_data() -> void:
	Global.players[0]["name"] = player_one_name.text
	Global.players[0]["color"] = Global.COLORS[p_one_colours.selected]

	var selected_colour: int
	var selected_name: String

	match game_type:
		"Multiplayer":
			selected_colour = p_two_colours.selected
			selected_name = player_two_name.text
		"Singleplayer":
			selected_colour = bot_two_colours.selected
			selected_name = "Bot"
			AlgorithmManager.set_chosen_algorithm(1, bot_two_algorithms.selected)
		"BotVBot":
			Global.players[0]["name"] = "Bot One"
			Global.players[0]["color"] = Global.COLORS[bot_one_colours.selected]
			selected_colour = bot_two_colours.selected
			selected_name = "Bot Two"
			AlgorithmManager.set_chosen_algorithm(0, bot_one_algorithms.selected)
			AlgorithmManager.set_chosen_algorithm(1, bot_two_algorithms.selected)
		"Q_Learning":
			Global.players[0]["color"] = Global.COLORS[0]
			Global.players[1]["color"] = Global.COLORS[1]
			selected_colour = bot_two_colours.selected
			selected_name = "Bot Two"

	Global.players[1]["color"] = Global.COLORS[selected_colour]
	Global.players[1]["name"] = selected_name


func setup_board_options() -> void:
	player_containers.map(func(x: VBoxContainer): x.hide())
	global_options.map(func(x: Control): x.show())
	game_type_dict[game_type].map(func(x: Control): x.show())


## Hide previous menu, add new panel to stack
func _on_menu_button_pressed(menu: Control, prev_button: Button = null) -> void:
	menu_stack.back().hide()
	menu_stack.append(menu)
	menu.show()

	if !prev_button:
		return

	game_type = prev_button.name.replace("Button", "")
	setup_board_options()


## Hide current menu, pop stack and reveal last shown
func _on_back_button_pressed() -> void:
	menu_stack.back().hide()
	menu_stack.pop_back()
	menu_stack.back().show()


## Override the game type as no other board options will be needed
func _on_qlearning_game_pressed() -> void:
	game_type = "Q_Learning"
	_on_start_game_pressed()


## Generate the board
func _on_start_game_pressed() -> void:
	board_options_menu.hide()
	set_game_data()
	var game: BaseGame = Resources.get_resource(game_type.to_lower() + "_game").instantiate()
	Global.game = game
	Global.coloured_fences = fence_coloured_button.is_pressed()
	game.setup_board(board_dimensions)

	background.add_child(game, true)


func _on_exit_button_pressed() -> void:
	Global.game.queue_free()
	AlgorithmManager.qlearning.FreeQTable()
	AlgorithmManager.save_algorithm_data()
	show_main_menu()


## Ensure both players cannot be the same colour.
func _on_colour_selected(index: int, prev_index: int, other_button: OptionButton) -> void:
	# Loop through all options and enable them
	for i in range(other_button.get_item_count()):
		other_button.set_item_disabled(i, false)

	# Re-enable the previously selected colour in the other option button
	other_button.set_item_disabled(prev_index, false)
	# Disable the selected colour in the other option button
	other_button.set_item_disabled(index, true)



func _on_colour_toggle_toggled(toggled_on: bool) -> void:
	fence_coloured_button.text = "Coloured" if toggled_on else "Gray"
