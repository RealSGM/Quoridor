class_name Tile extends Button

## NESW Cardinal Tiles
var connections: Array[Tile] = [null, null, null, null]
var pawns: Array[Pawn] = [null, null]
var pawn_moved: bool = false


func _ready() -> void:
	pressed.connect(SignalManager.tile_pressed.emit.bind(self))


func clear_pawns() -> void:
	if pawn_moved:
		pawn_moved = false
		return
	
	pawns[0].hide()
	pawns[0].modulate.a = 1
	pawns[1].hide()
	pawns[1].modulate.a = 1
