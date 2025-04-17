extends PanelContainer

@export var q_learning: QLearning
@export var agent_counter: SpinBox

var num_agents: int = 10

func _on_train_button_pressed() -> void:
	q_learning.TrainQAgent(num_agents)




func _on_spin_box_value_changed(value: float) -> void:
	num_agents = int(value)
