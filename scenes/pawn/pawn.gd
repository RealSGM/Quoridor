class_name Pawn extends Panel

var player_name: String = ''
var current: TileButton

var colour: Color:
	set(val):
		colour = val
		modulate = val
