class_name Board extends Control

# TODO Add UI Button for Pawn Movement
# - Disable all tiles (don't darken)
# Move Pawn Setup
	# - Enable adjacent tiles
	# - Darken the non-adjacent tiles
	
# TODO UI 
# - Show current player's Turn
# - Move History
	
# TODO Fence block pawn movement

@export_category("Board")
@export var board_anchor: Control
@export var tile_container: GridContainer
@export var fence_button_container: GridContainer

@export_category("User Interface")
@export var toggle_direction_button: Button
@export var move_pawn_button: Button
@export var place_fence_button: Button
@export var confirm_button: Button
@export var exit_button: Button

var fence_buttons: Array[FenceButton] = []
var tile_buttons: Array[Tile] = []

## Stored as [Player One Tile, Player Two Tile]
var current_tiles: Array[Tile] = [null, null]
var current_player: int = 0

@onready var selected_fence_button: FenceButton = null:
	set(val):
		if selected_fence_button:
			selected_fence_button.clear_fences()
		selected_fence_button = val
		set_confirm_button(val, selected_pawn_tile)

@onready var selected_pawn_tile: Tile = null:
	set(val):
		if selected_pawn_tile:
			selected_pawn_tile.clear_pawns()
		selected_pawn_tile = val
		set_confirm_button(selected_fence_button, val)
		
		if val:
			var pawn: Pawn = val.pawns[current_player]
			pawn.modulate.a = 0.5
			pawn.show()


func _ready() -> void:
	_on_directional_button_pressed()
	
	exit_button.pressed.connect(SignalManager.exit_pressed.emit)
	SignalManager.tile_pressed.connect(_on_tile_pressed)


## Setup the board with the selected size
func setup_board(board_size: int) -> void:
	instance_tiles(board_size)
	instance_fence_buttons(board_size - 1)
	spawn_pawns(board_size)
	reset_board(Color.TRANSPARENT)


## Set the grid container size and instance the tiles under the grid
func instance_tiles(board_size: int) -> void:
	var tile_resource: Resource  = Resources.get_resource('tile')
	var total_tiles: int = board_size * board_size
	tile_container.columns = board_size
	
	for i: int in range(total_tiles):
		var tile: Tile = tile_resource.instantiate()
		#tile.set_disabled(true)
		tile_container.add_child(tile, true)
		tile_buttons.append(tile)
	
	for index: int in range(total_tiles):
		var curr_tile: Tile = tile_buttons[index]
		
		if index >= board_size: # Check for North Tile
			curr_tile.connections[0] = tile_buttons[index - board_size]
		if (index + 1) % board_size != 0: # Check for East Tile
			curr_tile.connections[1] = tile_buttons[index + 1]
		if index < total_tiles - board_size: # Check for South Tile
			curr_tile.connections[2] = tile_buttons[index + board_size]
		if index % board_size != 0: # Check for West Tile
			curr_tile.connections[3] = tile_buttons[index - 1]


## Set the fence container size and instance the fence buttons under the 
## container
func instance_fence_buttons(fence_size: int) -> void:
	var fence_button_resource: Resource  = Resources.get_resource('fence_button')
	var total_fences: int = fence_size * fence_size
	var board_size: int = fence_size + 1
	fence_button_container.columns = fence_size
	
	for i: int in range(fence_size * fence_size):
		var fence_button: Button = fence_button_resource.instantiate()
		fence_buttons.append(fence_button)
		fence_button_container.add_child(fence_button, true)
	
	for index: int in range(total_fences):
		var curr_fence: FenceButton = fence_buttons[index]
		
		if index >= fence_size: # Check for North Tile
			curr_fence.connections[0] = fence_buttons[index - fence_size]
		if (index + 1) % fence_size != 0: # Check for East Tile
			curr_fence.connections[1] = fence_buttons[index + 1]
		if index < total_fences - fence_size: # Check for South Tile
			curr_fence.connections[2] = fence_buttons[index + fence_size]
		if index % fence_size != 0: # Check for West Tile
			curr_fence.connections[3] = fence_buttons[index - 1]
	
		# Left Horizontal
		curr_fence.tile_connections[0][0] = [tile_buttons[index], tile_buttons[index + board_size]]
		# Right Horizontal
		curr_fence.tile_connections[0][1] = [tile_buttons[index + 1], tile_buttons[index + 1 + board_size]]
		# Top Vertical
		curr_fence.tile_connections[1][0] = [tile_buttons[index + 1], tile_buttons[index + 1]]
		# Bottom Vertical
		curr_fence.tile_connections[1][1] = [tile_buttons[index + board_size], tile_buttons[index + board_size + 1]]


func set_confirm_button(fence_button: FenceButton, tile_button: Tile) -> void:
	confirm_button.disabled = !(fence_button || tile_button)


func update_fence_buttons() -> void:
	for fence_button: FenceButton in fence_buttons:
		fence_button.disabled = fence_button.dir_is_disabled[Global.dir_index]
		# Disable mouse filter if the button is disabled
		fence_button.mouse_filter = Control.MOUSE_FILTER_IGNORE if fence_button.disabled else Control.MOUSE_FILTER_STOP
	# TODO Implement illegal fence check


@warning_ignore("integer_division")
@warning_ignore("narrowing_conversion")
func spawn_pawns(board_size: int) -> void:
	# For each tile, instance a pawn for both players
	for index: int in range(tile_buttons.size()):
		var player_one: Pawn = spawn_pawn(Global.player_one["name"], Global.player_one["color"], index)
		var player_two: Pawn = spawn_pawn(Global.player_two["name"], Global.player_two["color"], index)
		tile_buttons[index].pawns[0] = player_one
		tile_buttons[index].pawns[1] = player_two
	
	current_tiles[0] = tile_buttons[board_size * (board_size - 0.5)]
	current_tiles[1] = tile_buttons[board_size / 2]
	
	# Show both pawns
	current_tiles[0].pawns[0].show()
	current_tiles[1].pawns[1].show()


func spawn_pawn(p_name: String, color: Color, pos_index: int) -> Pawn:
	var pawn: Pawn = Resources.get_resource("pawn").instantiate()
	pawn.colour = color
	pawn.player_name = p_name
	tile_buttons[pos_index].add_child(pawn, true)
	pawn.current = tile_buttons[pos_index]
	pawn.hide()
	return pawn


func set_fence_buttons(color: Color) -> void:
	fence_button_container.get_children().map(func(x: FenceButton): x.self_modulate = color)


func reset_board(color: Color) -> void:
	tile_buttons.map(func(tile: Tile): 
		tile.disabled = true
		tile.modulate = Color.WHITE
	)
	set_fence_buttons(color)


@warning_ignore('incompatible_ternary')
func set_non_adjacent_tiles(current_tile: Tile, set_disabled: bool) -> void:
	for tile: Tile in tile_buttons:
		# Disable the current tile, but do not darken
		if tile == current_tile:
			tile.disabled = true
		# Enable the current tiles
		elif tile in current_tile.connections:
			tile.disabled = false
			tile.modulate = Color.WHITE
		else:
			tile.modulate = Color(0.7, 0.7, 0.7) if set_disabled else Color.WHITE
			tile.disabled = set_disabled
			tile.focus_mode = Control.FOCUS_NONE if set_disabled else Control.FOCUS_CLICK


# Signals ----------------------------------------------------------------------
func _on_fence_button_pressed(fence_button: FenceButton) -> void:
	selected_fence_button = fence_button
	selected_pawn_tile = null
	fence_button.h_fence.visible = Global.dir_index == 0
	fence_button.v_fence.visible = Global.dir_index != 0


## Flip the rotation of the fence
func _on_directional_button_pressed() -> void:
	selected_fence_button = null
	Global.dir_index = 1 - Global.dir_index
	toggle_direction_button.text = 'Horizontal' if Global.dir_index == 0 else 'Vertical'
	update_fence_buttons()


func _on_move_pawn_pressed() -> void:
	set_non_adjacent_tiles(current_tiles[current_player], true)
	set_fence_buttons(Color.TRANSPARENT)
	selected_fence_button = null


func _on_place_fence_pressed() -> void:
	set_non_adjacent_tiles(current_tiles[current_player], false)
	reset_board(Color.WHITE)
	selected_pawn_tile = null


func _on_confirm_pressed() -> void:
	# Reset Board
	reset_board(Color.TRANSPARENT)
	
	if selected_fence_button:
		# Flip the index (for NESW adjustment)
		var flipped_index: int = 1 - Global.dir_index
		# Get the adjacent directionals
		var disabled_indexes: Array[int] = [flipped_index, flipped_index + 2]
		
		# Disable the adjacents buttons, for that direction
		for indexes: int in disabled_indexes:
			if selected_fence_button.connections[indexes]:
				selected_fence_button.connections[indexes].dir_is_disabled[Global.dir_index] = true
		
		# Check which tile connnections should be removed
		var connections: Array[Array] = selected_fence_button.tile_connections
		
		# Loop through the connections in the directed fence index
		for connection: Array in connections[Global.dir_index]:
			for index: int in connection.size():
				var tile: Tile = connection[index]
				var inverted_index: int = 1 - index
				var tile_to_remove: Tile = connection[inverted_index]
				var replace_index: int =  tile.connections.find(tile_to_remove)
				tile.connections[replace_index] = null
		
		selected_fence_button.fence_placed = true
		selected_fence_button = null
	
	elif selected_pawn_tile:
		# Move the pawn
		current_tiles[current_player].pawns[current_player].hide()
		selected_pawn_tile.pawns[current_player].modulate.a = 1
		
		# Update Pawn
		current_tiles[current_player] = selected_pawn_tile
		
		# Reset selected pawn, enabled pawn moved so the new pawn isn't hidden
		selected_pawn_tile.pawn_moved = true 
		selected_pawn_tile = null
		
	update_fence_buttons()
	
	# Switch to next player
	current_player = 1 - current_player


func _on_tile_pressed(tile: Tile) -> void:
	selected_pawn_tile = tile
