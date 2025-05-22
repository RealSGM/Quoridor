extends Node

const BITS: Array[int] = [0, 1]
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
var players: Array[Dictionary] = [{}, {}, {"color": Color.GHOST_WHITE}]
var game: BaseGame

var is_outputting: bool = true

func help() -> String:
	Console.show_help()
	return ""


func clear() -> String:
	Console.clear()
	return ""


func evaluate_board(player: int = 0) -> String:
	var evaluation = game.board_wrapper.EvaluateBoard(player)
	return "Evaluation: " + str(evaluation)


func get_fences(direction: int = 0) -> String:
	var fence_bitboard = game.board_wrapper.GetFencesAsArray(direction)
	var bb_string: String = ""

	for i: int in range(fence_bitboard.size()):
		if i % 8 == 0 and i != 0:
			bb_string += "\n"
		bb_string += " " + str(fence_bitboard[i]) + " "

	return bb_string


func get_enabled_fences(direction: int = 0) -> String:
	var fence_bitboard = game.board_wrapper.GetEnabledFencesAsArray(direction)
	var bb_string: String = ""

	for i: int in range(fence_bitboard.size()):
		if i % 8 == 0 and i != 0:
			bb_string += "\n"
		bb_string += " " + str(fence_bitboard[i]) + " "

	return bb_string


func get_fence_corners(tile: int = 0) -> String:
	var corners = Helper.GetFenceCorners(tile)
	var msg: String = ""

	for i: int in range(corners.size()):
		msg += str(corners[i]) + " "

	return msg


func toggle_output() -> String:
	is_outputting = !is_outputting
	return "Outputting to Console is %s" % ["enabled" if is_outputting else "disabled"]
