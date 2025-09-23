using Godot;

namespace MoF.Addons.ScenesManager
{

	/// <summary>
	/// A transition node that adds a color overlay effect during scene transitions.
	/// Inherits from <see cref="TransitionNode"/> and uses a <see cref="ColorRect"/> to display a customizable color overlay.
	/// </summary>
	[Tool, GlobalClass]
	public partial class TransitionNodeWithColor : TransitionNode
	{
		/// <summary>
		/// ColorRect used to display the transition color overlay.
		/// </summary>
		[Export]
		private ColorRect TransitionColorRect { get; set; }

		/// <summary>
		/// Sets or gets the color used for the transition overlay.
		/// </summary>
		/// <value>
		/// The color of the transition overlay.
		/// </value>
		public Color TransitionColor
		{
			set
			{
				TransitionColorRect.Color = value;
				// If using a shader, update the shader parameter as well
				if (TransitionColorRect.Material is ShaderMaterial)
					TransitionColorRect.Material.Set("shader_parameter/color", value);
			}
			get
			{
				return TransitionColorRect.Color;
			}
		}

		/// <summary>
		/// Called when the transition is ready to begin.
		/// Validates the AnimationPlayer, sets up scene containers, and starts the animation.
		/// </summary>
		public override void _TransitionReady()
		{
			base.SetupAnimationPlayer();
			base.SetupTargetSceneRoot();
			base.SetupCurrentSceneRoot();
			SetupColorLayer();

			base.AnimationPlayer.Play("TRANSITION");
		}

		protected void SetupColorLayer()
		{
			if (TransitionColorRect == null)
			{
				TransitionColorRect = new ColorRect { Name = "ColorLayer" };
				AddChild(TransitionColorRect);
				TransitionColorRect.Owner = this;
			}
		}

	}
}
