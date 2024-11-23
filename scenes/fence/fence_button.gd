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


func set_tile_connections(index: int, board_size: int, tiles: Array[Tile]) -> void:
	tile_connections[0][0] = [tiles[index], tiles[index + board_size]] # Left Horizontal
	tile_connections[0][1] = [tiles[index + 1], tiles[index + 1 + board_size]] # Right Horizontal
	tile_connections[1][0] = [tiles[index + 1], tiles[index + 1]] # Top Vertical
	tile_connections[1][1] = [tiles[index + board_size], tiles[index + board_size + 1]] # Bottom Vertical


func clear_fences() -> void:
	if fence_placed:
		return
	
	h_fence.visible = false
	v_fence.visible = false
