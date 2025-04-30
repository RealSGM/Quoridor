class_name QLearningTraining extends BaseGame

@export var run_simulation_button: Button
@export var speeds: Array[float] = [0, 0.01, 0.05, 0.1, 0.2]
@export var waits: Array[float] = [0, 0.05, 0.1, 0.25, 0.5]

var is_running: bool = false
var wait_time: float = waits[2]

func _ready() -> void:
	SignalManager.training_finished.connect(_on_training_finished)
	SignalManager.move_selected.connect(_on_move_selected)
	AlgorithmManager.qlearning.LoadQTable("")
	_on_speed_h_slider_value_changed(2)
	_on_wait_h_slider_value_changed(2)
	_on_epsilon_h_slider_value_changed(0.5)
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

	board_wrapper.AddMove(move_code)
	move_code = ""
	current_player = 1 - current_player


#region Signals
#endregion


func _on_run_simulation_button_pressed() -> void:
	is_running = !is_running

	run_simulation_button.text = "Run Simulation" if !is_running else "Stop Simulation"
	Console.add_entry("Running QLearning Training " if is_running else "Paused QLearning Training", 0)

	if is_running && !AlgorithmManager.qlearning.isRunning:
		user_interface._on_reset_button_pressed()
		AlgorithmManager.qlearning.TrainSingleEpisode()


func _on_save_q_table_pressed() -> void:
	Console.add_entry("Force saving QTable", 0)
	AlgorithmManager.qlearning.SaveQTable("")


func _on_prune_button_pressed() -> void:
	Console.add_entry("Force pruning QTable", 0)
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


func _on_training_finished(winner: int) -> void:
	if winner in Global.BITS:
		user_interface.update_win(winner)
	else:
		user_interface.win_label.text = "Error / Draw"
		is_running = false # NOTE This is for debugging
	if is_running:
		await get_tree().create_timer(wait_time).timeout
		user_interface._on_reset_button_pressed()
		_on_prune_button_pressed()
		_on_save_q_table_pressed()
		AlgorithmManager.qlearning.TrainSingleEpisode()


func _on_speed_h_slider_value_changed(value: float) -> void:
	AlgorithmManager.qlearning.simulationDelay = speeds[value]


func _on_epsilon_h_slider_value_changed(value: float) -> void:
	AlgorithmManager.qlearning.epsilon = value


func _on_wait_h_slider_value_changed(value: float) -> void:
	wait_time = waits[value]
