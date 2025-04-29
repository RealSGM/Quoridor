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

var is_outputting: bool = false
var dump_console_to_file: bool = false


func help() -> String:
	Console.show_help()
	return ""


func clear() -> String:
	Console.clear()
	return ""


func evaluate_board(player: int = 0) -> String:
	var evaluation = game.board.EvaluateBoard(player)
	return "Evaluation: " + str(evaluation)


func get_moves(player: int = 0) -> String:
	var moves = game.board.GetAllMovesSmart(player)
	var msg: String = ""

	for i: int in range(moves.size()):
		msg += str(moves[i]) + ",  "

	return msg


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


func is_fence_legal(player: int = 0, dir: int = 0, index: int = 0) -> String:
	var is_illegal = IllegalFenceCheck.IsFenceIllegal(game.board, player, dir, index)
	var text = "Player " + str(player) + " fence at " + str(index) + " is "

	if is_illegal:
		text += "illegal"
	else:
		text += "legal"

	return text


func toggle_dump() -> String:
	dump_console_to_file = !dump_console_to_file
	return "Dumping console to file is %s" % ["enabled" if dump_console_to_file else "disabled"]


func toggle_output() -> String:
	is_outputting = !is_outputting
	return "Outputting to Console is %s" % ["enabled" if is_outputting else "disabled"]


func test() -> String:
	print_orphan_nodes()
	return ""
