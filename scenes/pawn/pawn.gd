class_name Pawn extends Button

var player_name: String = ''

var colour: Color:
	set(val):
		colour = val
		modulate = val
