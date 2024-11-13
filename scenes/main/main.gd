extends Control

@export var board_size: int = 9

@export_category("Menus")
@export var main_menu: PanelContainer
@export var play_menu: PanelContainer
@export var multiplayer_menu: PanelContainer
@export var board_options_menu: PanelContainer

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

@export_category("Board")
@export var grid_container: GridContainer

var menu_stack: Array = []

@onready var menus: Array[PanelContainer] = [main_menu, play_menu, multiplayer_menu, board_options_menu]
@onready var tile_resource: Resource = Resources.get_resource('tile')
 
func _ready() -> void:
	setup_menus()


func setup_menus() -> void:
	menus.map(func(menu: PanelContainer): menu.hide())
	main_menu.show()
	menu_stack.append(main_menu)
	
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


func setup_board() -> void:
	grid_container.columns = board_size
	for i: int in range(board_size * board_size):
		var tile: Tile = tile_resource.instantiate()
		tile.set_disabled(true)
		grid_container.add_child(tile, true)


func _on_menu_button_pressed(menu: PanelContainer) -> void:
	menu_stack.back().hide()
	menu_stack.append(menu)
	menu.show()


func _on_back_button_pressed() -> void:
	menu_stack.back().hide()
	menu_stack.pop_back()
	menu_stack.back().show()


func _on_start_game_pressed() -> void:
	pass
