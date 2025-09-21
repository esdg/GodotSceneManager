using Godot;
using MoF.Addons.ScenesManager.Enums;

namespace MoF.Addons.ScenesManager.Scripts.Resources
{
    /// <summary>
    /// Represents a signal slot for scene transitions in the Scene Manager.
    /// Contains information about the output slot, target scene, transition, and related modifiers.
    /// </summary>
    [Tool]
    public partial class SceneManagerOutSlotSignal : Resource
    {
        /// <summary>
        /// Gets or sets the index of the output slot signal.
        /// </summary>
        [Export] public int Index { get; set; }

        /// <summary>
        /// Gets or sets the name of the output slot signal.
        /// </summary>
        [Export] public string OutSlotSignalName { get; set; }

        /// <summary>
        /// Gets or sets the target scene for the transition.
        /// </summary>
        [Export] public TargetScene TargetScene { get; set; } = new();

        /// <summary>
        /// Gets or sets the type of the target scene.
        /// </summary>
        [Export] public TargetSceneType TargetSceneType { get; set; } = new();

        /// <summary>
        /// Gets or sets the file name of the transition to use.
        /// </summary>
        [Export] public string TransitionFileName { get; set; }

        /// <summary>
        /// Gets or sets the transition modifier for the scene transition.
        /// </summary>
        [Export] public TransitionModifier TransitionModifier { get; set; } = new();
    }
}