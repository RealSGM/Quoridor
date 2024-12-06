class_name FenceButton extends Button

@export var h_fence: Panel
@export var v_fence: Panel

## Stops the fences being hidden
var fence_placed: bool = false

## Stores if the button should be disabled for each direction
## based off it the adjacent fence is placed or not
var dir_disabled: Array[bool] = [false, false]
## Stores if the button should be disabled for each direction
## based off latest DFS
var dfs_disabled: Array[bool] = [false, false]

var id: int


func _ready() -> void:
	pressed.connect(Global.game._on_fence_button_pressed.bind(id))
	h_fence.hide()
	v_fence.hide()


func clear_fences() -> void:
	if fence_placed:
		return
	
	h_fence.visible = false
	v_fence.visible = false
