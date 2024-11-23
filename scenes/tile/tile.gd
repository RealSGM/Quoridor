class_name Tile extends Button

## NESW Cardinal Tiles
var connections: Array[Tile] = [null, null, null, null]
var pawns: Array[Pawn] = [null, null]
var pawn_moved: bool = false


func _ready() -> void:
	pressed.connect(SignalManager.tile_pressed.emit.bind(self))


func set_connections(index: int, board_size: int, tiles: Array[Tile]) -> void:
	if index >= board_size: # Check for North Tile
		connections[0] = tiles[index - board_size]
	if (index + 1) % board_size != 0: # Check for East Tile
		connections[1] = tiles[index + 1]
	if index < board_size * (board_size - 1): # Check for South Tile
		connections[2] = tiles[index + board_size]
	if index % board_size != 0: # Check for West Tile
		connections[3] = tiles[index - 1]


func clear_pawns() -> void:
	if pawn_moved:
		pawn_moved = false
		return
	
	pawns[0].hide()
	pawns[0].modulate.a = 1
	pawns[1].hide()
	pawns[1].modulate.a = 1
