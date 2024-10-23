class_name Tile extends Control

@export_category("Nodes")
@export var tile_button: Button
@export var north_button: Button
@export var south_button: Button
@export var east_button: Button
@export var west_button: Button

func _ready() -> void:
	## Not sure if this is needed yet
	north_button.connect('pressed', FenceManager._fence_pressed.bind(north_button, self))
	south_button.connect('pressed', FenceManager._fence_pressed.bind(south_button, self))
	east_button.connect('pressed', FenceManager._fence_pressed.bind(east_button, self))
	west_button.connect('pressed', FenceManager._fence_pressed.bind(west_button, self))

## Runs when a tile is pressed, for pawn movement
func _on_tile_button_pressed() -> void:
	pass
