using System.IO;
using Godot;
using Godot.Collections;

namespace MoF.Addons.ScenesManager.Scripts.Resources
{

    [Tool, GlobalClass]
    public partial class SceneManagerSettings : Resource
    {
        [Export] public string SceneManagerSchemaPath { get; set; }
    }
}