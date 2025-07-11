class_name TileButton extends Button

@export var label: Label

var pawns: Array[Panel] = [null, null]
var pawn_moved: bool = false
var id: int


func _ready() -> void:
	pressed.connect(Global.game._on_tile_pressed.bind(id))
	label.text = str(id)


func clear_pawns() -> void:
	if pawn_moved:
		pawn_moved = false
		return

	for i: int in Global.BITS:
		pawns[i].modulate.a = 1
		pawns[i].hide()
