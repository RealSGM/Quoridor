class_name QLearningTraining extends BaseGame

@export var run_single_button: Button
@export var run_multiple_button: Button
@export var speeds: Array[float] = [0.05, 0.1, 0.2, 0.5, 1]

var num_agents: int = 2

func _ready() -> void:
	SignalManager.training_finished.connect(_on_training_finished)
	SignalManager.move_selected.connect(_on_move_selected)
	super._ready()


func _on_confirm_pressed() -> void:
	reset_board_tiles()
	
	var index: int = move_code.split("_")[0].substr(2).to_int()
	var direction: int = 1 if index < 0 else 0
	index = abs(index)
	
	match move_code[1]:
		"f":
			confirm_place_fence(index, direction)
		"m":
			confirm_move_pawn(index)

	board.AddMove(move_code)
	move_code = ""
	
	IllegalFenceCheck.GetIllegalFences(board, 1 - current_player)
	current_player = 1 - current_player


#region Signals
#endregion

func _on_spin_box_value_changed(value: float) -> void:
	num_agents = int(value)


func _on_run_simulation_button_pressed() -> void:
	user_interface._on_reset_button_pressed()
	Console.add_entry("Running simulation of one episode", 0)
	AlgorithmManager.qlearning.LoadQTable("")
	AlgorithmManager.qlearning.TrainSingleEpisode(true)


func _on_run_multiple_button_pressed() -> void:
	run_multiple_button.disabled = true
	Console.add_entry("Running %s episodes" % [num_agents], 0)
	AlgorithmManager.qlearning.TrainQAgent(num_agents)


func _on_save_q_table_pressed() -> void:
	Console.add_entry("Force saving QTable", 0)
	AlgorithmManager.qlearning.SaveQTable("")
	

func _on_prune_button_pressed() -> void:
	AlgorithmManager.qlearning.PruneQTable(0)


func confirm_place_fence(fence: int, direction: int) -> void:
	_on_fence_button_pressed(abs(fence), direction)
	super.confirm_place_fence(fence, direction)


func confirm_move_pawn(tile: int) -> void:
	_on_tile_pressed(tile)
	super.confirm_move_pawn(tile)


func _on_move_selected(move: String) -> void:
	move_code = move
	_on_confirm_pressed()


func _on_training_finished() -> void:
	run_multiple_button.disabled = false
	

func _on_h_slider_value_changed(value: float) -> void:
	AlgorithmManager.qlearning.simulationDelay = speeds[value]
