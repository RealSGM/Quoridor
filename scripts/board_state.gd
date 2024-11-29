class_name BoardState extends Control

var fence_counts: Array[int] = [10, 10]

## 1D Array containing every fence button
var fences: Array[Fence] = []
## 1D Array containing every tile
var tiles: Array[Tile] = []
## Stores the adjacent cardinal directions relative to the board
var directions: Array[int] = []

## Stored as [Player One Index, Player Two Index]
var pawn_indexes: Array[int] = [-1, -1]
## Stores the upper and lower bounds for each pawn's finish line
var win_indexes: Array[Array] = [[], []]
