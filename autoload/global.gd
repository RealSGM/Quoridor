extends Node

const COLORS: Array[Color] = [
	Color.LIGHT_CORAL,
	Color.LIGHT_GREEN,
	Color.CORNFLOWER_BLUE,
	Color.LIGHT_GOLDENROD,
]

## O is horizontal, 1 is vertical
var dir_index: int = 1
var board: Board
var main: Control

var player_one: Dictionary = {
	"color": Color.WHITE,
	"name": "Player 1"
}

var player_two: Dictionary = {
	"color": Color.WHITE,
	"name": "Player 2"
}
