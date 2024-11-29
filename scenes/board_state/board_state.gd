class_name BoardState extends Control

var fence_counts: Array[int] = [10, 10]

## Stored as [Player One Tile, Player Two Tile]
var current_tiles: Array[Tile] = [null, null]
## 1D Array containing every fence button
var fence_buttons: Array[FenceButton] = []
## 1D Array containing every tile
var tile_buttons: Array[Tile] = []
## Stores the adjacent cardinal directions relative to the board
var directions: Array[int] = []
## Stores the upper and lower bounds for each pawn's finish line
var win_indexes: Array[Array] = [[], []]

func add_tile() -> void:
	var tile: Tile = Tile.new()
	tile_buttons.append(tile)
	pass


func set_current_tile(player: int, tile: Tile) -> void:
	current_tiles[player] = tile
