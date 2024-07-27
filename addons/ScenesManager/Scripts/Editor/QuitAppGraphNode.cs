#if TOOLS
using Godot;
using Godot.Collections;
using MoF.Addons.ScenesManager.Constants;

namespace MoF.Addons.ScenesManager.Scripts.Editor
{
    [Tool]
    public partial class QuitAppGraphNode : ScenesManagerBaseGraphNode
    {
        private static Texture2D quitAppIconTexture;
        private static StyleBoxFlat quitAppGraphNodeStylePanel;
        private static StyleBoxFlat quitAppGraphNodeStyleTitlebar;
        private Node outSlot;

        public override Array<string> OutSignalsNames
        {
            get
            {
                var signals = new Array<string> { "Quit App Out slot" };
                return signals;
            }
        }

        public override void _LoadResources()
        {
            quitAppIconTexture ??= ResourceLoader.Load<Texture2D>(Plugin.PathToPlugin + AddonConstants.GraphNode.QuitAppGraphNode.StartingAppIconTexturePath);
            quitAppGraphNodeStylePanel ??= ResourceLoader.Load<StyleBoxFlat>(Plugin.PathToPlugin + AddonConstants.GraphNode.QuitAppGraphNode.GraphNodeStylePanelPath);
            quitAppGraphNodeStyleTitlebar ??= ResourceLoader.Load<StyleBoxFlat>(Plugin.PathToPlugin + AddonConstants.GraphNode.QuitAppGraphNode.GraphNodeStyleTitlebarPath);
        }

        public override void _SetupGraphNode()
        {
            Title = AddonConstants.GraphNode.QuitAppGraphNode.Title;
            Size = AddonConstants.GraphNode.QuitAppGraphNode.InitialSize;
            Set("theme_override_constants/separation", AddonConstants.GraphNode.NodeVerticalSpace);
            Set("theme_override_styles/panel", quitAppGraphNodeStylePanel);
            Set("theme_override_styles/titlebar", quitAppGraphNodeStyleTitlebar);
        }

        public override void _ReadyNode()
        {
            TextureRect textureRect = new()
            {
                Texture = quitAppIconTexture,
                ExpandMode = TextureRect.ExpandModeEnum.KeepSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,

            };
            AddChild(textureRect);
            SetSlot(textureRect.GetIndex(), true, 0, AddonConstants.GraphNode.QuitAppGraphNode.Color, false, 0, AddonConstants.GraphNode.QuitAppGraphNode.Color);
            EmitSignal(SignalName.GraphNodeReady);
        }
    }
}
#endif