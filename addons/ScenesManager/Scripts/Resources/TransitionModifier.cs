using Godot;

namespace MoF.Addons.ScenesManager.Scripts.Resources
{
    /// <summary>
    /// Represents a modifier for scene transitions, such as speed.
    /// </summary>
    [Tool]
    public partial class TransitionModifier : Resource
    {
        /// <summary>
        /// Gets or sets the speed of the transition.
        /// </summary>
        [Export] public float Speed = 1.0f;

        [Export] public Color Color = Colors.Black;
    }
}