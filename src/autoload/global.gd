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
var chosen_algorithms: Array = [null, null]


func help() -> String:
	Console.show_help()
	return " "


func clear() -> String:
	Console.clear()
	return ""


func evaluate_board(is_maximising: bool) -> String:
	if !game:
		return "Game not started"
	return "Evaluation: %s, Last Move: %s" % [game.board.EvaluateBoard(is_maximising), game.board.GetLastMove()]


func get_move_history() -> String:
	return game.board.GetMoveHistory()


func get_goal_tiles() -> String:
	if !game:
		return "Game not started"

	var text: String = ""

	for player: int in Global.BITS:
		text += "Player %s: %s\n" % [player, game.board.GetGoalTiles(player)]

	return text


func get_fence_amounts() -> String:
	if !game:
		return "Game not started"

	var text: String = ""

	for player: int in Global.BITS:
		text += "Player %s: %s\n" % [player, game.board.GetFenceCount(player)]

	return text


func get_illegal_fences() -> String:
	var illegal_fences: PackedInt32Array = game.board.GetIllegalFences()
	var text: String = ""

	for i: int in range(illegal_fences.size()):
		var fence: int = illegal_fences[i]
		var fence_string: String = "X" if fence == -1 else str(fence)
		text += " [%s]" % [fence_string]
		if (i + 1) % (game.board.BoardSize - 1) == 0:
			text += "\n"
	return text


func get_shortest_path(player: int) -> String:
	if !game:
		return "Game not started"

	var text: String = ""
	var path: PackedInt32Array = game.board.GetShortestPath(player)

	for i: int in range(path.size()):
		text += " %s " % path[i]

	return text


func get_next_move(is_maximising: bool = true, depth: int = 1) -> String:
	if !game:
		return "Game not started"

	MiniMaxAlgorithm.max_depth = depth
	var move: String = MiniMaxAlgorithm.GetMove(game.board, game.current_player, is_maximising, false)

	return "Best Move: %s" % [move]


func print_fences() -> String:
	if !game:
		return "Game not started"

	var text: String = ""

	var amount: int = game.board.GetFenceAmount()

	for index: int in range(amount):
		var fence_string: String = ""
		match game.board.GetPlacedFence(index):
			0:
				fence_string = "-"
			1:
				fence_string = "│"
			-1:
				fence_string = "•"

		text += " [%s] " % [fence_string]
		if (index + 1) % (game.board.BoardSize - 1) == 0:
			text += "\n"

	return text


func print_tiles() -> String:
	if !game:
		return "Game not started"

	var text: String = ""

	for index: int in range(game.board.GetBoardSize() * game.board.GetBoardSize()):
		var tile: PackedInt32Array = game.board.GetTile(index)
		var connections_string: String = ""

		for connection in tile:
			connections_string += " %s " % connection

		text += "[ %s ]" % connections_string

		if (index + 1) % game.board.BoardSize == 0:
			text += "\n\n"
	return text
