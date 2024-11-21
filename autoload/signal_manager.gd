extends Node
## Simple Signal Manager, allows for signals to be connected globally across the project

signal exit_pressed
signal tile_pressed

## Helper connect function for when a nodepath is the same as the script
func connect_signal(signal_name: String, nodepath: Object, method: String) -> void:
	var _err = connect(signal_name, Callable(nodepath, method))
	
	
## Helper connect function for when a nodepath is the same as the script
func disconnect_signal(signal_name: String, nodepath: Object, method : String) -> void:
	disconnect(signal_name,Callable(nodepath,method))
