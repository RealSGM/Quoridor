class_name FenceButton extends Button

@export var v_fence: Panel
@export var h_fence: Panel

var fence_placed: bool = false

func _ready() -> void:
	pressed.connect(Global.board._on_fence_button_pressed.bind(self))
	h_fence.hide()
	v_fence.hide()


func clear_fences() -> void:
	if fence_placed:
		return
	
	h_fence.visible = false
	v_fence.visible = false
