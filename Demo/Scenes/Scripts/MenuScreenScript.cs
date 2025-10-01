using Godot;
using System;

public partial class MenuScreenScript : Control
{
	[Signal] public delegate void StartGameSceneEventHandler();
	[Signal] public delegate void GoToSettingsEventHandler();
	[Signal] public delegate void QuitGameEventHandler();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}


	private void _on_settings_pressed_settings()
	{
		EmitSignal(SignalName.GoToSettings);
	}
	private void _on_start_game_pressed()
	{
		EmitSignal(SignalName.StartGameScene);
	}


	private void _on_quit_btn_pressed()
	{
		EmitSignal(SignalName.QuitGame);
	}
}
