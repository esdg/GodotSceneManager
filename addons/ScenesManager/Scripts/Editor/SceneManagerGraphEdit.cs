#if TOOLS
using Godot;

namespace MoF.Addons.ScenesManager.Scripts.Editor
{
	/// <summary>
	/// Represents the main GraphEdit component for the Scenes Manager editor.
	/// </summary>
	[Tool]
	public partial class SceneManagerGraphEdit : GraphEdit
	{
		/// <summary>
		/// Called when the node enters the scene tree for the first time.
		/// </summary>
		public override void _Ready()
		{
		}

		/// <summary>
		/// Called every frame. <paramref name="delta"/> is the elapsed time since the previous frame.
		/// </summary>
		/// <param name="delta">Elapsed time since the previous frame.</param>
		public override void _Process(double delta)
		{
		}
	}
}
#endif



