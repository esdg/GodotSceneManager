using Godot;
using System;

public partial class Scene_C : Control
{
	[Signal] public delegate void GoToPreviousSceneEventHandler();
	[Signal] public delegate void GoToNextSceneEventHandler();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _on_button_pressed()
	{
		EmitSignal(SignalName.GoToPreviousScene);
	}

	private void _on_button_pressed_next()
	{
		EmitSignal(SignalName.GoToNextScene);
	}
}






