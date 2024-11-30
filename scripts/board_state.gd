class_name BoardState extends Control

var fence_counts: Array[int] = [10, 10]

## Stored as [Player One Index, Player Two Index]
@export var pawn_indexes: Array[int] = [-1, -1]
## 1D Array containing every fence button
@export var fences: Array[Fence] = []
## 1D Array containing every tile
@export var tiles: Array[Tile] = []
## Stores the adjacent cardinal directions relative to the board
var directions: Array[int] = []

## Stores the upper and lower bounds for each pawn's finish line
var win_indexes: Array[Array] = [[], []]


func place_fence(fence: Fence, direction: int) -> void:
	# Loop through the connections in the directed fence index
	for connection: Array in fence.adj_tiles[direction]:
		for index: int in connection.size():
			remove_tile_connection(connection, index)


func remove_tile_connection(connection: Array, index) -> void:
	var tile: Tile = connection[index]
	var inverted_index: int = 1 - index
	var tile_to_remove: Tile = connection[inverted_index]
	var replace_index: int =  tile.connections.find(tile_to_remove)
	tile.connections[replace_index] = null
