class_name Tile extends Node

## NESW Cardinal Tiles
var connections: Array[Tile] = [null, null, null, null]
var button: TileButton

func set_connections(index: int, board_size: int, tiles: Array[Tile]) -> void:
	if index >= board_size: # Check for North Tile
		connections[0] = tiles[index - board_size]
	if (index + 1) % board_size != 0: # Check for East Tile
		connections[1] = tiles[index + 1]
	if index < board_size * (board_size - 1): # Check for South Tile
		connections[2] = tiles[index + board_size]
	if index % board_size != 0: # Check for West Tile
		connections[3] = tiles[index - 1]
