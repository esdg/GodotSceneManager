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
			set => TransitionColorRect.Color = value;
			get => TransitionColorRect.Color;
		}
	}
}
