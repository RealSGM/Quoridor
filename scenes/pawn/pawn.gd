class_name Pawn extends Panel

var player_name: String = ''
var current: Tile

var colour: Color:
	set(val):
		colour = val
		modulate = val
