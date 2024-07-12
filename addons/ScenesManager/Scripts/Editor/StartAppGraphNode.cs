using Godot;
using Godot.Collections;
using MoF.Addons.ScenesManager.Constants;

namespace MoF.Addons.ScenesManager
{
    [Tool, GlobalClass]
    public partial class StartAppGraphNode : ScenesManagerBaseGraphNode
    {
        private static Texture2D startingAppIconTexture;
        private static StyleBoxFlat startingAppGraphNodeStylePanel;
        private static StyleBoxFlat startingAppGraphNodeStyleTitlebar;
        private Node outSlot;

        public override Array<string> OutSignals
        {
            get
            {
                var signals = new Array<string> { "Starting App Out slot" };
                return signals;
            }
        }

        public override void _LoadResources()
        {
            startingAppIconTexture ??= ResourceLoader.Load<Texture2D>(AddonConstants.GraphNode.StartAppGraphNode.StartingAppIconTexturePath);
            startingAppGraphNodeStylePanel ??= ResourceLoader.Load<StyleBoxFlat>(AddonConstants.GraphNode.StartAppGraphNode.GraphNodeStylePanelPath);
            startingAppGraphNodeStyleTitlebar ??= ResourceLoader.Load<StyleBoxFlat>(AddonConstants.GraphNode.StartAppGraphNode.GraphNodeStyleTitlebarPath);
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
            Selectable = false;
            Draggable = false;
            TextureRect textureRect = new()
            {
                Texture = startingAppIconTexture,
                ExpandMode = TextureRect.ExpandModeEnum.KeepSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,

            };
            AddChild(textureRect);
            SetSlot(textureRect.GetIndex(), false, 0, Colors.White, true, 0, AddonConstants.GraphNode.OutSlotColor);
        }
    }
}