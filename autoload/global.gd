extends Node

const COLORS: Array[Color] = [
	Color.RED,
	Color.GREEN,
	Color.BLUE,
	Color.YELLOW,
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
