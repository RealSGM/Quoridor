class_name Fence extends Node

## Stores tile adj_fences as [[Horizontal Fence], [Vertical Fence]]
var adj_tiles: Array[Array] = [[[],[]], [[],[]]]
var button: FenceButton
var adj_fences: Array[Fence] = [null, null, null, null]
var id: int

func set_fence_connections(index: int, fence_size: int, fences: Array[Fence]) -> void:
	if index >= fence_size: # Check for North Tile
		adj_fences[0] = fences[index - fence_size]
	if (index + 1) % fence_size != 0: # Check for East Tile
		adj_fences[1] = fences[index + 1]
	if index < fence_size * (fence_size - 1): # Check for South Tile
		adj_fences[2] = fences[index + fence_size]
	if index % fence_size != 0: # Check for West Tile
		adj_fences[3] = fences[index - 1]


@warning_ignore("integer_division")
func set_tile_connections(index: int, board_size: int) -> void:
	# Match the fence button index to the index of the Top-Left Tile in 2x2 Grid
	index += (index / (board_size - 1))
	
	var TL: int = index
	var TR: int = index + 1
	var BL: int = index + board_size
	var BR: int = index + 1 + board_size
	
	adj_tiles[0][0] = [TL, BL] # Left Horizontal Fence
	adj_tiles[0][1] = [TR, BR] # Right Horizontal Fence
	
	adj_tiles[1][0] = [TL, TR] # Top Vertical Fence
	adj_tiles[1][1] = [BL, BR] # Bottom Vertical Fence
