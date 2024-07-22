using Godot;
using System;

public partial class GameOverScript : Control
{
	[Signal] public delegate void QuitToMenuEventHandler();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}


	private void _on_back_to_menu_btn_pressed()
	{
		EmitSignal(SignalName.QuitToMenu);
	}
}


