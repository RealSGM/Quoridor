extends Node

const COLORS: Array[Color] = [
	Color.LIGHT_CORAL,
	Color.LIGHT_GREEN,
	Color.CORNFLOWER_BLUE,
	Color.GOLD,
]

## O is horizontal, 1 is vertical
var fence_direction: int = 0
var game: BaseGame
var board_size: int
var main: Control

var players: Array[Dictionary] = [{}, {}]
