using System.Threading.Tasks;
using Godot;
using MoF.Addons.ScenesManager.Scripts;

namespace MoF.Addons.ScenesManager
{
	[Tool, GlobalClass]
	public partial class JumpCutTransitionCanvas : TransitionCanvasBase
	{
		public override void PlayInAnimation()
		{
			EmitSignal(SignalName.InAnimationFinished);
		}

		public override void PlayOutAnimation()
		{
			EmitSignal(SignalName.OutAnimationFinished, TargetScene);
		}

		public override void _ExitTree()
		{
			QueueFree();
		}
	}
}
