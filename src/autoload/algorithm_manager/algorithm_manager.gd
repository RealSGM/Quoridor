extends Node

const DATA: Dictionary = {
	"games_played": 0,
	"wins": 0,
	"moves_made": 0, # Total
	"pawn_moves": 0, # Total
	"current_turn": 0, # Reset value for each game
	"move_speeds_cumulative": [], # Sum of move speeds done per each round
	"fences_remaining_cumulative": [], # Sum of fences remaining per each round
	"nodes_searched_cumulative": [], # Sum of nodes searched per each round (Minimax only)
}

const TOTAL_LIST: Array = [
	"games_played",
	"wins",
	"moves_made",
	"pawn_moves",
	"current_turn",
]

@export var algorithms: Array[Node]
@export var minimax: MiniMaxAlgorithm
@export var mcts: MCTSAlgorithm
@export var qlearning: QLearningAlgorithm

var chosen_algorithms: Array = [null, null]
var algorithm_data: Dictionary[Node, Dictionary] = {}


func _ready() -> void:
	SignalManager.data_collected.connect(_update_stat)

	algorithm_data = {
		minimax: DATA.duplicate(true),
		mcts: DATA.duplicate(true),
		qlearning: DATA.duplicate(true)
	}


func set_chosen_algorithm(arr_index: int, algo_index: int) -> void:
	chosen_algorithms[arr_index] = algorithms[algo_index]


## Run the chosen algorithm, using the BoardWrapper
func run(wrapper, player: int) -> void:
	chosen_algorithms[player].GetMove(wrapper, player)


func _update_stat(node: Node, stat: String, val: Variant) -> void:
	if TOTAL_LIST.has(stat):
		algorithm_data[node][stat] += val
		return

	var current_turn = algorithm_data[node]["current_turn"]

	if algorithm_data[node][stat].size() <= current_turn:
		algorithm_data[node][stat].append(val)
	else:
		algorithm_data[node][stat][current_turn] += val
