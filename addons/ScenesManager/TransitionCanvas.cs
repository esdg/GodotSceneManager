using System;
using Godot;
using MoF.Addons.ScenesManager.Scripts;

namespace MoF.Addons.ScenesManager
{
	[Tool, GlobalClass]
	public partial class TransitionCanvas : TransitionCanvasBase
	{
		[Export]
		private AnimationPlayer AnimationPlayer { get; set; }

		public Node CurrentSceneRoot
		{
			set => _currentSceneNode = value;
		}

		public override void _TransitionReady()
		{
			if (!ValidateAnimationPlayer()) return;

			SetupTargetSceneRoot();
			SetupCurrentSceneRoot();

			AnimationPlayer.Play("TRANSITION");
		}

		private bool ValidateAnimationPlayer()
		{
			if (AnimationPlayer == null)
			{
				GD.PrintErr("'AnimationPlayer' property field is empty, add one in the inspector field");
				return false;
			}

			if (!AnimationPlayer.HasAnimation("TRANSITION"))
			{
				GD.PrintErr("Could not find animation named 'TRANSITION' in 'AnimationPlayer', create new animation in the 'AnimationPlayer' and name it 'TRANSITION'");
				return false;
			}

			return true;
		}

		private void SetupTargetSceneRoot()
		{
			if (!HasNode("target_scene"))
			{
				AddSceneNode(_targetSceneRoot, "target_scene");
			}

			_targetSceneRoot = GetNode<Control>("target_scene");

			if (_targetPackedScene == null)
			{
				AddDummySceneNode(_targetSceneRoot, "target_scene", Colors.MediumPurple, "Scene B");
			}
			else
			{
				_targetSceneNode = _targetPackedScene.Instantiate();
				_targetSceneRoot.AddChild(_targetSceneNode);
				_targetSceneNode.Name = TargetNodeName;
			}
		}

		private void SetupCurrentSceneRoot()
		{
			if (!HasNode("current_scene"))
			{
				AddSceneNode(_currentSceneRoot, "current_scene");
			}

			_currentSceneRoot = GetNode<Control>("current_scene");

			if (_currentSceneNode == null)
			{
				AddDummySceneNode(_currentSceneRoot, "current_scene", Colors.MediumSeaGreen, "Scene A");
			}
			else
			{
				_currentSceneRoot.AddChild(_currentSceneNode);
			}
		}

		private void AddSceneNode(Control sceneNode, string nodeName)
		{
			AddChild(sceneNode);
			sceneNode.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			sceneNode.Owner = this;
			sceneNode.Name = nodeName;
		}

		private static void AddDummySceneNode(Control sceneNode, string nodeName, Color backgroundColor, string labelText)
		{
			var background = new ColorRect { Color = backgroundColor };
			background.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			sceneNode.AddChild(background);

			var label = new Label
			{
				Text = labelText,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			label.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			label.AddThemeFontSizeOverride("font_size", 50);
			background.AddChild(label);
		}
	}
}
