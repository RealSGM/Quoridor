class_name TileButton extends Button

var pawns: Array[Panel] = [null, null]
var pawn_moved: bool = false
var id: int

func _ready() -> void:
	pressed.connect(Global.game._on_tile_pressed.bind(id))


func clear_pawns() -> void:
	if pawn_moved:
		pawn_moved = false
		return
	
	pawns[0].hide()
	pawns[0].modulate.a = 1
	pawns[1].hide()
	pawns[1].modulate.a = 1
