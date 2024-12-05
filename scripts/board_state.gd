class_name BoardState extends Control

var fence_counts: Array[int] = [10, 10]

## Stored as [Player One Index, Player Two Index]
var pawn_indexes: Array[int] = [-1, -1]
## 1D Array containing every fence button
var fences: Array[Fence] = []
## 1D Array containing every tile
var tiles: Array[Tile] = []
## Stores the adjacent cardinal directions relative to the board
var adj_offsets: Array[int] = []
## Stores the upper and lower bounds for each pawn's finish line
var win_indexes: Array[Array] = [[], []]


func place_fence(fence: Fence, direction: int) -> void:
	# Loop through the connections in the directed fence index
	for connection: Array in fence.adj_tiles[direction]:
		for index: int in connection.size():
			remove_tile_connection(connection, index)


func remove_tile_connection(connection: Array, index) -> void:
	var tile_index = connection[index]
	var inverted_index: int = 1 - index
	var removing_index: int = connection[inverted_index]
	var tile: Tile = tiles[tile_index]
	tile.connections[tile.connections.find(removing_index)] = -1
