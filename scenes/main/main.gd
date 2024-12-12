extends Control

@export_category("Menus")
@export var main_menu: PanelContainer
@export var play_menu: PanelContainer
@export var multiplayer_menu: PanelContainer
@export var board_options_menu: PanelContainer
@export var foreground: ColorRect

@export_category("Main Menu Buttons")
@export var play_button: Button
@export var exit_button: Button

@export_category("Play Menu Buttons")
@export var singleplayer_button: Button
@export var multiplayer_button: Button
@export var play_back_button: Button

@export_category("Multiplayer Buttons")
@export var local_button: Button
@export var online_button: Button
@export var multiplayer_back_button: Button

@export_category("Board Options")
@export var board_vbc: VBoxContainer

@export var start_game_button: Button
@export var board_options_back_button: Button

# Global Options
@export var board_size_container: HBoxContainer
@export var size_option_button: OptionButton
@export var player_one_container: VBoxContainer
@export var p_one_colours: OptionButton
@export var player_one_name: LineEdit

# Local Options
@export var p_two_colours: OptionButton
@export var player_two_name: LineEdit
@export var player_two_container: VBoxContainer

# Singleplayer Options
@export var bot_container: VBoxContainer
@export var bot_colours: OptionButton

@export var size_options: Array[int] = [7, 9, 11]
@export var board_dimensions: float = 800

var menu_stack: Array = []
var game_type: String

@onready var menus: Array[PanelContainer] = [main_menu, play_menu, multiplayer_menu, board_options_menu]
@onready var global_options: Array[BoxContainer] = [player_one_container, board_size_container]
@onready var game_type_dict: Dictionary = {
	"Singleplayer": [bot_container],
	"Local": [player_two_container]
}


func _ready() -> void:
	setup_menus()
	SignalManager.exit_pressed.connect(_on_exit_button_pressed)


## Setup the menus, ensure all panels are hidden on launch
## Connect all buttons to appropriate signals
func setup_menus() -> void:
	menus.map(func(menu: PanelContainer): menu.hide())
	show_main_menu()
	
	play_button.pressed.connect(_on_menu_button_pressed.bind(play_menu))
	exit_button.pressed.connect(get_tree().quit)
	
	singleplayer_button.pressed.connect(_on_menu_button_pressed.bind(board_options_menu, singleplayer_button))
	multiplayer_button.pressed.connect(_on_menu_button_pressed.bind(multiplayer_menu))
	play_back_button.pressed.connect(_on_back_button_pressed)
	
	#online_button.pressed
	local_button.pressed.connect(_on_menu_button_pressed.bind(board_options_menu, local_button))
	multiplayer_back_button.pressed.connect(_on_back_button_pressed)
	
	start_game_button.pressed.connect(_on_start_game_pressed)
	board_options_back_button.pressed.connect(_on_back_button_pressed)
	
	# Setup colour option buttons
	p_one_colours.item_selected.connect(_on_colour_selected.bind(p_one_colours.selected,  p_two_colours,))
	p_two_colours.item_selected.connect(_on_colour_selected.bind(p_two_colours.selected, p_one_colours))
	bot_colours.item_selected.connect(_on_colour_selected.bind(bot_colours.selected, p_one_colours))
	p_one_colours.select(0)
	p_two_colours.select(1)
	
	setup_board_sizes()


## Setup the option button for the board sizes
func setup_board_sizes() -> void:
	for board_size_option: int in size_options:
		size_option_button.add_item(str(board_size_option) + " * " + str(board_size_option))
	size_option_button.selected = 1
	size_option_button.clear_radio_boxes()


func show_main_menu() -> void:
	menu_stack.clear()
	main_menu.show()
	menu_stack.append(main_menu)


func set_game_data() -> void:
	Global.players[0]['name'] = player_one_name.text
	Global.players[0]['color'] = Global.COLORS[p_one_colours.selected]
	
	var selected_colour: int 
	var selected_name: String
	
	match game_type:
		"Local":
			selected_colour = p_two_colours.selected
			selected_name = player_two_name.text
		"Singleplayer":
			selected_colour = bot_colours.selected
			selected_name = "Bot"
	
	Global.players[1]['color'] = Global.COLORS[selected_colour]
	Global.players[1]['name'] = selected_name


func setup_board_options() -> void:
	board_vbc.get_children().map(func(x: Control): x.hide())
	global_options.map(func(x: Control): x.show())
	game_type_dict[game_type].map(func(x: Control): x.show())


## Hide previous menu, add new panel to stack
func _on_menu_button_pressed(menu: PanelContainer, prev_button: Button = null) -> void:
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


## Generate the board
func _on_start_game_pressed() -> void:
	board_options_menu.hide()
	set_game_data()
	var game: BaseGame = Resources.get_resource(game_type.to_lower() + "_game").instantiate()
	Global.game = game
	game.setup_board(size_options[size_option_button.selected])
	foreground.add_child(game, true)
	game.board.scale = Vector2.ONE * float(board_dimensions) / float(game.board_container.size.x)


func _on_exit_button_pressed() -> void:
	Global.game.queue_free()
	show_main_menu()


## Ensure both players cannot be the same colour.
func _on_colour_selected(index: int, prev_index: int, other_button: OptionButton) -> void:
	# Disable the selected colour in the other option button
	other_button.set_item_disabled(index, true)
	# Re-enable the previously selected colour in the other option button
	other_button.set_item_disabled(prev_index, false)
