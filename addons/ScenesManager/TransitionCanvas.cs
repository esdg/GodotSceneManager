using Godot;
using MoF.Addons.ScenesManager.Scripts;

namespace MoF.Addons.ScenesManager
{
	[Tool, GlobalClass]
	public partial class TransitionCanvas : TransitionCanvasBase
	{

		[Export] private AnimationPlayer AnimationPlayer { get; set; }

		public override void _Ready()
		{
			if (!AnimationPlayer.HasAnimation("IN") || !AnimationPlayer.HasAnimation("OUT"))
			{
				GD.PrintErr($"");
				return;
			}
			AnimationPlayer.AnimationFinished += OnAnimationFinished;
			// AddChild(CurrentScene);
			// CurrentScene.Owner = this;
			// CurrentScene.Name = "current_scene";
			// AddChild(TargetScene);
			// TargetScene.Owner = this;
			// TargetScene.Name = "target_scene";
		}

		public override void PlayInAnimation()
		{
			AnimationPlayer.Play("IN");
		}

		public override void PlayOutAnimation()
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
				EmitSignal(SignalName.OutAnimationFinished, TargetScene);
			}
		}

		public override void _ExitTree()
		{
			AnimationPlayer.AnimationFinished -= OnAnimationFinished;
			QueueFree();
		}
	}
}
