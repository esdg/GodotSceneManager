#if TOOLS
using Godot;
using Godot.Collections;
using MoF.Addons.ScenesManager.Constants;

namespace MoF.Addons.ScenesManager.Scripts.Editor
{
    [Tool]
    public partial class StartAppGraphNode : ScenesManagerBaseGraphNode
    {
        private static Texture2D startingAppIconTexture;
        private static StyleBoxFlat startingAppGraphNodeStylePanel;
        private static StyleBoxFlat startingAppGraphNodeStyleTitlebar;
        private Node outSlot;

        public override Array<string> OutSignalsNames
        {
            get
            {
                var signals = new Array<string> { "Starting App Out slot" };
                return signals;
            }
        }

        public override void _LoadResources()
        {
            startingAppIconTexture ??= ResourceLoader.Load<Texture2D>(Plugin.PathToPlugin + AddonConstants.GraphNode.StartAppGraphNode.StartingAppIconTexturePath);
            startingAppGraphNodeStylePanel ??= ResourceLoader.Load<StyleBoxFlat>(Plugin.PathToPlugin + AddonConstants.GraphNode.StartAppGraphNode.GraphNodeStylePanelPath);
            startingAppGraphNodeStyleTitlebar ??= ResourceLoader.Load<StyleBoxFlat>(Plugin.PathToPlugin + AddonConstants.GraphNode.StartAppGraphNode.GraphNodeStyleTitlebarPath);
        }

        public override void _SetupGraphNode()
        {
            Title = AddonConstants.GraphNode.StartAppGraphNode.Title;
            Size = AddonConstants.GraphNode.StartAppGraphNode.InitialSize;
            Set("theme_override_constants/separation", AddonConstants.GraphNode.NodeVerticalSpace);
            Set("theme_override_styles/panel", startingAppGraphNodeStylePanel);
            Set("theme_override_styles/titlebar", startingAppGraphNodeStyleTitlebar);
        }

        public override void _ReadyNode()
        {
            TextureRect textureRect = new()
            {
                Texture = startingAppIconTexture,
                ExpandMode = TextureRect.ExpandModeEnum.KeepSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,

            };
            AddChild(textureRect);
            SetSlot(textureRect.GetIndex(), false, 0, AddonConstants.GraphNode.StartAppGraphNode.Color, true, 0, AddonConstants.GraphNode.StartAppGraphNode.Color);
            EmitSignal(SignalName.GraphNodeReady);
        }
    }
}
#endif