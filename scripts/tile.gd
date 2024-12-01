class_name Tile extends Node

## NESW Cardinal Tiles
var connections: Array[int] = [-1, -1, -1, -1]
var button: TileButton
var id: int


func set_connections(index: int, board_size: int) -> void:
	connections[0] = index - board_size if index >= board_size else -1 # North Tile
	connections[1] = index + 1 if (index + 1) % board_size != 0 else -1 # East Tile
	connections[2] = index + board_size if index < board_size * (board_size - 1) else -1 # South Tile
	connections[3] = index - 1  if index % board_size != 0 else -1 # West Tile
