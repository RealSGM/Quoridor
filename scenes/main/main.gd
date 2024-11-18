extends Control

const SCALE_STEP: float = 0.25

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
@export var start_game_button: Button
@export var board_options_back_button: Button
@export var size_option_button: OptionButton
@export var size_options: Array[int] = [7, 9, 11]

var menu_stack: Array = []
var board: Board

@onready var menus: Array[PanelContainer] = [main_menu, play_menu, multiplayer_menu, board_options_menu]


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
	
	#singleplayer_button.pressed
	multiplayer_button.pressed.connect(_on_menu_button_pressed.bind(multiplayer_menu))
	play_back_button.pressed.connect(_on_back_button_pressed)
	
	#online_button.pressed
	local_button.pressed.connect(_on_menu_button_pressed.bind(board_options_menu))
	multiplayer_back_button.pressed.connect(_on_back_button_pressed)
	
	start_game_button.pressed.connect(_on_start_game_pressed)
	board_options_back_button.pressed.connect(_on_back_button_pressed)
	
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

# Signals ----------------------------------------------------------------------
## Hide previous menu, add new panel to stack
func _on_menu_button_pressed(menu: PanelContainer) -> void:
	menu_stack.back().hide()
	menu_stack.append(menu)
	menu.show()


## Hide current menu, pop stack and reveal last shown
func _on_back_button_pressed() -> void:
	menu_stack.back().hide()
	menu_stack.pop_back()
	menu_stack.back().show()


## Generate the board
func _on_start_game_pressed() -> void:
	var scale_size: Vector2 = Vector2.ONE * ((1 - size_option_button.selected) * SCALE_STEP + 1)
	board_options_menu.hide()
	
	board = Resources.get_resource("board").instantiate()
	Global.board = board
	board.board_anchor.scale = scale_size
	board.setup_board(size_options[size_option_button.selected])
	
	foreground.add_child(board, true)


func _on_exit_button_pressed() -> void:
	board.queue_free()
	show_main_menu()
