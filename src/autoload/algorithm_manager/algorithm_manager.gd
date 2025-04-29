extends Node

@export var algorithms: Array[Node]
@export var minimax: MiniMaxAlgorithm
@export var mcts: MCTSAlgorithm
@export var qlearning: QLearningAlgorithm

var chosen_algorithms: Array = [null, null]


func set_chosen_algorithm(arr_index: int, algo_index: int) -> void:
	chosen_algorithms[arr_index] = algorithms[algo_index]


func run(wrapper, player: int) -> void:
	chosen_algorithms[player].GetMove(wrapper, player)
