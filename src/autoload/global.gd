extends Node

const BITS: Array[int] = [0, 1]
const MAX_FENCES: int = 10
const BOARD_SIZE: int = 9

const COLORS: Array[Color] = [
	Color.LIGHT_CORAL,
	Color.LIGHT_GREEN,
	Color.CORNFLOWER_BLUE,
	Color.GOLD,
]

## O is horizontal, 1 is vertical
var fence_direction: int = 0
var coloured_fences: bool = false

# Third Element is for Testing
var players: Array[Dictionary] = [{}, {}, {"color": Color.GHOST_WHITE}]

var game: BaseGame
var algorithms: AlgorithmManager


func help() -> String:
	Console.show_help()
	return ""


func clear() -> String:
	Console.clear()
	return ""


func get_fences(direction: int = 0) -> String:
	var fence_bitboard = game.board.GetFencesAsArray(direction)
	var bb_string: String = ""

	for i: int in range(fence_bitboard.size()):
		if i % 8 == 0 and i != 0:
			bb_string += "\n"
		bb_string += " " + str(fence_bitboard[i]) + " "

	return bb_string
	

func get_enabled_fences(direction: int = 0) -> String:
	var fence_bitboard = game.board.GetEnabledFencesAsArray(direction)
	var bb_string: String = ""

	for i: int in range(fence_bitboard.size()):
		if i % 8 == 0 and i != 0:
			bb_string += "\n"
		bb_string += " " + str(fence_bitboard[i]) + " "

	return bb_string


func get_shortest_path(player: int = 0) -> String:
	var path = Algorithms.GetPathToGoal(game.board, player)

	var msg: String = "[ Start -> "

	for i: int in range(path.size()):
		msg += str(path[i]) + " -> "
	return msg + " End]"


func is_fence_legal(player: int = 0, dir: int = 0, index: int = 0) -> String:
	var is_legal = IllegalFenceCheck.IsFenceIllegal(game.board, player, dir, index)
	var text = "Player " + str(player) + " fence at " + str(index) + " is "

	if is_legal:
		text += "legal"
	else:
		text += "illegal"

	return text


func test(player: int = 0) -> String:
	game.board.Test(player)
	return ""


func get_fence_corners(tile: int = 0) -> String:
	var corners = Helper.GetFenceCorners(tile)
	var msg: String = ""

	for i: int in range(corners.size()):
		msg += str(corners[i]) + " "

	return msg


func get_adjacent_tiles(tile: int = 0) -> String:
	var adjacent_tiles = game.board.GetAdjacentTiles(tile)
	var msg: String = ""

	for i: int in range(adjacent_tiles.size()):
		msg += str(adjacent_tiles[i]) + " "

	return msg