extends Node

const FILE_PATH := "user://algorithm_data.json"

const DATA: Dictionary = {
	"games_played": 0,
	"wins": 0,
	"current_turn": 0, # Total
	"move_speeds_cumulative": [], # Sum of move speeds done per each round
	"fences_remaining_cumulative": [], # Sum of fences remaining per each round
	"nodes_searched_cumulative": [], # Sum of nodes searched per each round (Minimax only)
	"moves_made_cumulative": [], # Sum of moves made per each round
	"pawn_moves_cumulative": [], # Sum of pawn moves per each round
}

const TOTAL_LIST: Array = [
	"games_played",
	"wins",
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
	load_algorithm_data()


func set_chosen_algorithm(arr_index: int, algo_index: int) -> void:
	chosen_algorithms[arr_index] = algorithms[algo_index]


## Run the chosen algorithm, using the BoardWrapper
func run(wrapper, player: int) -> void:
	chosen_algorithms[player].GetMove(wrapper, player)


func save_algorithm_data() -> void:
	var json_ready: Dictionary[String, Dictionary] = {
		"minimax": algorithm_data[minimax],
		"mcts": algorithm_data[mcts],
		"qlearning": algorithm_data[qlearning]
	}

	var file: FileAccess = FileAccess.open(FILE_PATH, FileAccess.WRITE)
	file.store_string(JSON.stringify(json_ready, "\t"))
	file.close()
	file = null


func load_algorithm_data() -> void:
	var file: FileAccess = FileAccess.open(FILE_PATH, FileAccess.READ)
	var content = JSON.parse_string(file.get_as_text())

	file.close()
	file = null

	algorithm_data[minimax] = DATA.duplicate(true)
	algorithm_data[mcts] = DATA.duplicate(true)
	algorithm_data[qlearning] = DATA.duplicate(true)

	if not content:
		return

	if content.has("minimax"):
		algorithm_data[minimax] = content["minimax"]
	if content.has("mcts"):
		algorithm_data[mcts] = content["mcts"]
	if content.has("qlearning"):
		algorithm_data[qlearning] = content["qlearning"]


func _update_stat(node: Node, stat: String, val: Variant) -> void:
	if TOTAL_LIST.has(stat):
		algorithm_data[node][stat] += val
		return

	var current_turn = algorithm_data[node]["current_turn"]

	if algorithm_data[node][stat].size() <= current_turn:
		algorithm_data[node][stat].append(val)
	else:
		algorithm_data[node][stat][current_turn] += val
