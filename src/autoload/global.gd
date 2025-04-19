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
	return " "


func clear() -> String:
	Console.clear()
	return ""


func get_fences(direction: int) -> String:
	var fence_bitboard = game.board.GetFencesAsArray(direction)
	var bb_string: String = ""

	for i: int in range(fence_bitboard.size()):
		if i % 8 == 0 and i != 0:
			bb_string += "\n"
		bb_string += " " + str(fence_bitboard[i]) + " "

	return bb_string


func get_enabled_fences(direction: int) -> String:
	var fence_bitboard = game.board.GetEnabledFencesAsArray(direction)
	var bb_string: String = ""

	for i: int in range(fence_bitboard.size()):
		if i % 8 == 0 and i != 0:
			bb_string += "\n"
		bb_string += " " + str(fence_bitboard[i]) + " "

	return bb_string
