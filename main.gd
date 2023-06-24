extends Node


# Declare member variables here. Examples:
# var a: int = 2
# var b: String = "text"


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	$ViewportContainer/Viewport/ColorRect2.visible = $CheckBox.pressed


# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta: float) -> void:
#	pass


func _on_CheckBox_toggled(button_pressed: bool) -> void:
	if button_pressed:
		$ViewportContainer/Viewport/ColorRect2.show()
	else:
		$ViewportContainer/Viewport/ColorRect2.hide()
