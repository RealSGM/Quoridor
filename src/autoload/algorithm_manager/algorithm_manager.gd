extends Node

const FILE_PATH := "data/algorithm_data.json"

const DATA: Dictionary = {
	"games_played": 0,
	"wins": 0,
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
@export var random_ai: RandomAI

var chosen_algorithms: Array = [null, null]
var algorithm_nodes: Dictionary[Node, String] = {}
var current_algorithm_data: Dictionary[Node, Dictionary] = {}
var algorithm_data: Dictionary[Node, Dictionary] = {}


func _ready() -> void:
	SignalManager.data_collected.connect(_update_stat)

	load_algorithm_data()
	algorithm_nodes[minimax] = "minimax"
	algorithm_nodes[mcts] = "mcts"
	algorithm_nodes[qlearning] = "qlearning"
	algorithm_nodes[random_ai] = "randomai"


func set_chosen_algorithm(arr_index: int, algo_index: int) -> void:
	chosen_algorithms[arr_index] = algorithms[algo_index]


## Run the chosen algorithm, using the BoardWrapper
func run(wrapper, player: int) -> void:
	chosen_algorithms[player].GetMove(wrapper, player)


func save_algorithm_data() -> void:
	var json_ready: Dictionary[String, Dictionary] = {
		"minimax": algorithm_data[minimax],
		"mcts": algorithm_data[mcts],
		"qlearning": algorithm_data[qlearning],
		"randomai": algorithm_data[random_ai],
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
	algorithm_data[random_ai] = DATA.duplicate(true)

	if not content:
		return

	if content.has("minimax"):
		algorithm_data[minimax] = content["minimax"]
	if content.has("mcts"):
		algorithm_data[mcts] = content["mcts"]
	if content.has("qlearning"):
		algorithm_data[qlearning] = content["qlearning"]
	if content.has("randomai"):
		algorithm_data[random_ai] = content["randomai"]


func start_new_game() -> void:
	current_algorithm_data = {
		mcts: DATA.duplicate(true),
		minimax: DATA.duplicate(true),
		qlearning: DATA.duplicate(true),
		random_ai: DATA.duplicate(true)
	}

	for key in current_algorithm_data.keys():
		current_algorithm_data[key]["current_turn"] = 0


func end_game() -> void:
	# Save the current game data to the algorithm data
	for key in current_algorithm_data.keys():
		# Loop through the stats and add them to the algorithm data
		for stat in current_algorithm_data[key].keys():
			if stat == "current_turn":
				continue
			if stat in TOTAL_LIST:
				algorithm_data[key][stat] += current_algorithm_data[key][stat]
			else:
				for i in range(current_algorithm_data[key][stat].size()):
					if algorithm_data[key][stat].size() <= i:
						algorithm_data[key][stat].append(current_algorithm_data[key][stat][i])
					else:
						# Add the current algorithm data to the algorithm data
						algorithm_data[key][stat][i] += current_algorithm_data[key][stat][i]
	save_algorithm_data()


func _update_stat(node: Node, stat: String, val: Variant) -> void:
	if TOTAL_LIST.has(stat):
		current_algorithm_data[node][stat] += val
		return

	var current_turn = current_algorithm_data[node]["current_turn"]

	if current_algorithm_data[node][stat].size() <= current_turn:
		current_algorithm_data[node][stat].append(val)
	else:
		current_algorithm_data[node][stat][current_turn] += val
