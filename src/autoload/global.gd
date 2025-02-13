extends Node

const BITS: Array[int] = [0, 1]

const COLORS: Array[Color] = [
	Color.LIGHT_CORAL,
	Color.LIGHT_GREEN,
	Color.CORNFLOWER_BLUE,
	Color.GOLD,
]

## O is horizontal, 1 is vertical
var fence_direction: int = 0
var game: BaseGame
# Third Element is for Testing
var players: Array[Dictionary] = [{}, {}, {"color": Color.GHOST_WHITE}]


func help() -> String:
	Console.show_help()
	return " "


func clear() -> String:
	Console.clear()
	return ""


func get_fences() -> String:
	if !game:
		return "Game not started"

	var text: String = ""

	var amount: int = game.board.GetFenceAmount()

	for index: int in range(amount):
		var fence_string: String = "-" if game.board.GetPlacedFence(index) == 0 else "|"
		text += " [%s %s] " % [fence_string]
		if (index + 1) % (game.board.BoardSize - 1) == 0:
			text += "\n"

	return text


func get_tiles() -> String:
	if !game:
		return "Game not started"

	var text: String = ""

	for index: int in range(game.board.GetBoardSize() * game.board.GetBoardSize()):
		var tile: PackedInt32Array = game.board.GetTileConnections(index)
		var connections_string: String = ""

		for connection in tile:
			connections_string += " %s " % connection

		text += "[ %s ]" % connections_string

		if (index + 1) % game.board.BoardSize == 0:
			text += "\n\n"
	return text


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
