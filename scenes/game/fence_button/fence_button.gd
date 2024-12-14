class_name FenceButton extends Button

@export var h_fence: Panel
@export var v_fence: Panel

var id: int

func _ready() -> void:
	pressed.connect(Global.game._on_fence_button_pressed.bind(id))
	h_fence.hide()
	v_fence.hide()

func clear_fences() -> void:
	h_fence.visible = false
	v_fence.visible = false
