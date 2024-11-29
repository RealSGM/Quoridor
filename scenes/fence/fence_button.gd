class_name FenceButton extends Button

@export var h_fence: Panel
@export var v_fence: Panel

var fence_placed: bool = false
var dir_is_disabled: Array[bool] = [false, false]
var fence: Fence

func _ready() -> void:
	pressed.connect(Global.game._on_fence_button_pressed.bind(self))
	h_fence.hide()
	v_fence.hide()


func clear_fences() -> void:
	if fence_placed:
		return
	
	h_fence.visible = false
	v_fence.visible = false
