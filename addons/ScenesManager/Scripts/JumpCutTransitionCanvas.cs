using System.Threading.Tasks;
using Godot;
using MoF.Addons.ScenesManager.Scripts;

namespace MoF.Addons.ScenesManager
{
	[Tool, GlobalClass]
	public partial class JumpCutTransitionCanvas : TransitionCanvasBase
	{
		public override void _TransitionReady()
		{
			_targetSceneNode = _targetPackedScene.Instantiate();
			_targetSceneNode.Name = TargetNodeName;
			SendTransitionFinishedSignal();
		}
	}
}
