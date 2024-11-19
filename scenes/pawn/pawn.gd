class_name Pawn extends Panel

var player_name: String = ''

var colour: Color:
	set(val):
		colour = val
		modulate = val
