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


func _ready() -> void:
	var foo: Board = Board.new()
	foo.Initialise()
	
	foo.GetAdjacentTiles(53)

func help() -> String:
	Console.show_help()
	return " "


func clear() -> String:
	Console.clear()
	return ""


func evaluate_board(is_maximising: bool) -> String:
	if !game:
		return "Game not started"
		
	var evaluation: int = game.board.EvaluateBoard(is_maximising)
	var last_move: String = game.board.GetLastMove()
	return "Evaluation: %s, Last Move: %s" % [evaluation, last_move]


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


func get_shortest_path(player: int) -> String:
	if !game:
		return "Game not started"

	var text: String = ""
	var path: PackedInt32Array = game.board.GetShortestPath(player)

	for i: int in range(path.size()):
		text += " %s " % path[i]

	return text


func print_fences() -> String:
	if !game:
		return "Game not started"

	var text: String = ""

	var amount: int = (Global.BOARD_SIZE - 1) * (Global.BOARD_SIZE - 1)

	for index: int in range(amount):
		var fence_string: String = ""
		match game.board.IsFencePlaced(index):
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

	for index: int in range(BOARD_SIZE * BOARD_SIZE):
		var tile: PackedInt32Array = game.board.GetTileConnections(index)
		var connections_string: String = ""

		for connection in tile:
			connections_string += " %s " % connection

		text += "Tile %s:  [ %s ] \n" % [index, connections_string]
	return text


func evaluate_all_moves() -> String:
	if !game:
		return "Game not started"

	var text: String = ""

	for move: String in game.board.GetAllMoves(game.current_player):
		var temp_state: BoardState = game.board.Clone()
		temp_state.AddMove(move)
		var evaluation: int = temp_state.EvaluateBoard(game.current_player)
		text += "Move %s: %s\n" % [move, evaluation]

	return text
