using Godot;
using MoF.Addons.ScenesManager.Scripts;

namespace MoF.Addons.ScenesManager
{
	[Tool]
	public partial class JumpCutTransitionNode : TransitionNodeBase
	{
		public override void _TransitionReady()
		{
			_targetSceneNode = _targetPackedScene.Instantiate();
			_targetSceneNode.Name = TargetNodeName;
			SendTransitionFinishedSignal();
		}
	}
}
