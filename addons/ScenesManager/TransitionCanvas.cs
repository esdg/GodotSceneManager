using Godot;

namespace MoF.Addons.ScenesManager
{
	[Tool, GlobalClass]
	public partial class TransitionCanvas : CanvasLayer
	{
		[Export] private AnimationPlayer AnimationPlayer { get; set; }

		[Signal] public delegate void InAnimationFinishedEventHandler();
		[Signal] public delegate void OutAnimationFinishedEventHandler();

		public override void _Ready()
		{
			if (!AnimationPlayer.HasAnimation("IN") || !AnimationPlayer.HasAnimation("OUT"))
			{
				GD.PrintErr($"");
				return;
			}
			AnimationPlayer.AnimationFinished += OnAnimationFinished;
		}

		public void PlayInAnimation()
		{
			AnimationPlayer.Play("IN");
		}

		public void PlayOutAnimation()
		{
			AnimationPlayer.Play("OUT");
		}

		private void OnAnimationFinished(StringName animName)
		{
			if (animName == "IN")
			{
				EmitSignal(SignalName.InAnimationFinished);
			}
			else
			{
				EmitSignal(SignalName.OutAnimationFinished);
			}
		}

		public override void _ExitTree()
		{
			AnimationPlayer.AnimationFinished -= OnAnimationFinished;
			QueueFree();
		}
	}
}
