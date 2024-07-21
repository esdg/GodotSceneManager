using Godot;

namespace MoF.Addons.ScenesManager.Scripts
{
    [Tool]
    public abstract partial class TransitionCanvasBase : CanvasLayer
    {

        [Signal] public delegate void InAnimationFinishedEventHandler();
        [Signal] public delegate void OutAnimationFinishedEventHandler(Control CurrentScene);
        public Control CurrentScene { get; set; } = new();
        public Control TargetScene { get; set; } = new();

        public abstract void PlayInAnimation();
        public abstract void PlayOutAnimation();
    }
}