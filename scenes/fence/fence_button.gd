class_name FenceButton extends Button

@export var h_fence: Panel
@export var v_fence: Panel

var fence_placed: bool = false
var dir_is_disabled: Array[bool] = [false, false]
var connections: Array[FenceButton] = [null, null, null, null]

## Stores tile connections as [[Horizontal Fence], [Vertical Fence]]
var tile_connections: Array[Array] = [[[],[]], [[],[]]]

func _ready() -> void:
	pressed.connect(Global.board._on_fence_button_pressed.bind(self))
	h_fence.hide()
	v_fence.hide()


func set_fence_connections(index: int, fence_size: int, fences: Array[FenceButton]) -> void:
	if index >= fence_size: # Check for North Tile
		connections[0] = fences[index - fence_size]
	if (index + 1) % fence_size != 0: # Check for East Tile
		connections[1] = fences[index + 1]
	if index < fence_size * (fence_size - 1): # Check for South Tile
		connections[2] = fences[index + fence_size]
	if index % fence_size != 0: # Check for West Tile
		connections[3] = fences[index - 1]

@warning_ignore("integer_division")
func set_tile_connections(index: int, board_size: int, tiles: Array[Tile]) -> void:
	# Match the fence button index to the index of the Top-Left Tile in 2x2 Grid
	index += (index / (board_size - 1))
	
	var TL: Tile = tiles[index]
	var TR: Tile = tiles[index + 1]
	var BL: Tile = tiles[index + board_size]
	var BR: Tile = tiles[index + 1 + board_size]
	
	tile_connections[0][0] = [TL, BL] # Left Horizontal Fence
	tile_connections[0][1] = [TR, BR] # Right Horizontal Fence
	
	tile_connections[1][0] = [TL, TR] # Top Vertical Fence
	tile_connections[1][1] = [BL, BR] # Bottom Vertical Fence


func clear_fences() -> void:
	if fence_placed:
		return
	
	h_fence.visible = false
	v_fence.visible = false
