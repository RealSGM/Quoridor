class_name FenceButton extends Button

@export var h_fence: Panel
@export var v_fence: Panel

var id: int


func _ready() -> void:
	pressed.connect(Global.game._on_fence_button_pressed.bind(id))
	h_fence.hide()
	v_fence.hide()

func clear_fences(fence_placed: bool) -> void:
	if fence_placed:
		return
	
	h_fence.visible = false
	v_fence.visible = false


#func get_enabled() -> bool:
	#return dir_disabled[Global.fence_direction] or dfs_disabled[Global.fence_direction] or fence_placed
