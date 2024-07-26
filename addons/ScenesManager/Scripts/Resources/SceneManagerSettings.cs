using Godot;

namespace MoF.Addons.ScenesManager.Scripts.Resources
{

    [Tool]
    public partial class SceneManagerSettings : Resource
    {
        [Export] public string SceneManagerSchemaPath { get; set; }
    }
}