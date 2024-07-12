using System.IO;
using Godot;
using Godot.Collections;

namespace MoF.Addons.ScenesManager.Scripts.Resources
{
    [Tool, GlobalClass]
    public partial class SceneManagerItem : SceneManagerBaseItem
    {
        [Export] public PackedScene Scene { get; set; }

    }

}