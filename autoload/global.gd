extends Node

const BITS: Array[int] = [0, 1]

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

## Converts fence direction which is [0, 1]
## To [-1, 1] for notation
func map_fence_direction(fence: int) -> int:
	return fence * ((2 * fence_direction) - 1)
