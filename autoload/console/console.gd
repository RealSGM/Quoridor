extends Window

## TODO Add colours to text
## Error - Red
## Warning - Yellow ?
## Success

const COLORS: Array[Color] = [
	Color("#B0BEC5"), # Default
	Color("#FF4C4C"), # Error
	Color("#D4EDDA"), # Success,
]

@export var history_vbc: VBoxContainer
@export var input: LineEdit

# History
var history: Array[String] = [""]
var current_input: String
var input_index: int = 1

# Autocomplete
var pre_auto_complete_input: String
var autocomplete_list: Array
var tab_count: int = -999
var shift_held: bool = false

## Allows for threaded functions to work with the console
var entry_buffer: Array[Array]

## Used to handle where the expression should process from
var base_inst := Global
var global_methods: Array

@onready var expression: Expression = Expression.new()


func _ready() -> void:
	hide()
	
	# Get methods within Global function, exclude _ready
	global_methods = base_inst.get_script().get_script_method_list().map(func(x): return x.name)
	global_methods.erase("_ready")


func _input(event: InputEvent) -> void:
	if event.is_action_pressed('toggle_console'):
		visible = !visible
	if not visible:
		return
	if event.is_action_pressed("shift"):
		shift_held = true
	elif event.is_action_released("shift"):
		shift_held = false
	elif event.is_action_pressed('console_up'):
		traverse_history(-1)
	elif event.is_action_pressed("console_down"):
		traverse_history(1)
	elif event.is_action_pressed("autocomplete"):
		autocomplete()


func parse_expresion(text: String) -> void:
	entry_buffer.append([text, 0])
	
	# Process the text
	var err: int = expression.parse(text)
	
	# Invalid Expression
	if err != OK:
		entry_buffer.append(["Invalid Command: " + expression.get_error_text(), 1])
		return
		
	# Successful Execution
	var result = expression.execute([], base_inst)
	if not expression.has_execute_failed():
		entry_buffer.append([str(result), 2])
		return
		
	# Error executing
	entry_buffer.append(["Invalid Function", 1])


func traverse_history(modifier: int) -> void:
	# Ignore empty history
	if history.is_empty():
		return
	
	# Update index
	input_index = clamp(input_index + modifier, 0, history.size() - 1)
	replace_input_text(history[input_index], true)


## Autocomplete function
## Gets all methods that starts with the current input
func autocomplete(modifier: int = 1) -> void:
	# Invert autocomplete
	if shift_held:
		modifier = -1
		
	if tab_count == -999:
		pre_auto_complete_input = current_input
		autocomplete_list = []
		for method: String in global_methods:
			if current_input.is_empty():
				autocomplete_list.append(method)
			elif method.begins_with(pre_auto_complete_input):
				autocomplete_list.append(method)
		tab_count = -1
		
	if autocomplete_list.is_empty():
		return
	
	tab_count = clamp(tab_count + modifier, 0, autocomplete_list.size())
	tab_count = tab_count % len(autocomplete_list)
	var current_method = autocomplete_list[tab_count]
	replace_input_text(current_method + "()", false)


func replace_input_text(text: String,  reset_tab: bool) -> void:
	input.grab_focus()
	input.text = text
	_on_input_text_changed(text, reset_tab)
	
	input.caret_column = 999999 # Hacky way of moving cursor to end of LineEdit


func add_entry(text: String, index: int = 0) -> void:
	var command: Label = Resources.get_resource('command').instantiate()
	command.text = text
	command.modulate = COLORS[index]
	history_vbc.add_child(command, true)


func show_help() -> void:
	var text: String = "Available Functions: \n"
	for method: String in global_methods:
		text += method + "()\n"
	entry_buffer.append([text, 0])


func clear() -> void:
	history_vbc.get_children().map(func(l: Label): l.queue_free())

#region Signals
#endregion

func _on_input_text_changed(text: String, reset_tab_count = true) -> void:
	current_input = text
	history[-1] = current_input
	tab_count = -999 if reset_tab_count else tab_count


func _on_input_text_submitted(text: String) -> void:
	# Ignore empty text
	if text.is_empty():
		return
		
	parse_expresion(text)
	
	# Add new entry for entry
	history.append("current_input") 
	
	input_index = history.size() - 1
	input.clear()


func _on_timer_timeout() -> void:
	if entry_buffer.is_empty():
		return
	var data: Array = entry_buffer.pop_front()
	add_entry(data[0], data[1])


func _on_visibility_changed() -> void:
	await get_tree().process_frame
	input.grab_focus()


func _on_close_requested() -> void:
	hide()
