extends OptionButton


func _ready() -> void:
	clear_radio_boxes()


func clear_radio_boxes() -> void:
	var pm: PopupMenu = get_popup()
	for i: int in pm.get_item_count():
		if pm.is_item_radio_checkable(i):
			pm.set_item_as_radio_checkable(i, false)
