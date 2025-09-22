#if TOOLS
using Godot;
using Godot.Collections;
using MoF.Addons.ScenesManager.Constants;
using MoF.Addons.ScenesManager.Helpers;

namespace MoF.Addons.ScenesManager.Scripts.Editor
{
    /// <summary>
    /// Represents a custom graph node for starting the application in the Scenes Manager editor.
    /// </summary>
    [Tool]
    public partial class StartAppGraphNode : ScenesManagerBaseGraphNode
    {
        /// <summary>
        /// Cached texture for the starting app icon.
        /// </summary>
        private static Texture2D startingAppIconTexture;

        /// <summary>
        /// Cached style for the graph node panel.
        /// </summary>
        private static StyleBoxFlat startingAppGraphNodeStylePanel;

        /// <summary>
        /// Cached style for the graph node titlebar.
        /// </summary>
        private static StyleBoxFlat startingAppGraphNodeStyleTitlebar;

        /// <summary>
        /// Reference to the output slot node.
        /// </summary>
        private Node outSlot;

        /// <summary>
        /// Gets the names of the output signals for this node.
        /// </summary>
        public override Array<string> OutSignalsNames
        {
            get
            {
                var signals = new Array<string> { "Starting App Out slot" };
                return signals;
            }
        }

        /// <summary>
        /// Loads required resources for the node, such as textures and styles.
        /// </summary>
        public override void _LoadResources()
        {
            startingAppIconTexture ??= ResourceLoader.Load<Texture2D>(Plugin.PathToPlugin + AddonConstants.GraphNode.StartAppGraphNode.StartingAppIconTexturePath);
            startingAppGraphNodeStylePanel ??= ResourceLoader.Load<StyleBoxFlat>(Plugin.PathToPlugin + AddonConstants.GraphNode.StartAppGraphNode.GraphNodeStylePanelPath);
            startingAppGraphNodeStyleTitlebar ??= ResourceLoader.Load<StyleBoxFlat>(Plugin.PathToPlugin + AddonConstants.GraphNode.StartAppGraphNode.GraphNodeStyleTitlebarPath);
        }

        /// <summary>
        /// Sets up the graph node's appearance and properties.
        /// </summary>
        public override void _SetupGraphNode()
        {
            Title = AddonConstants.GraphNode.StartAppGraphNode.Title;
            Size = AddonConstants.GraphNode.StartAppGraphNode.InitialSize;
            Set("theme_override_constants/separation", AddonConstants.GraphNode.NodeVerticalSpace);
            Set("theme_override_styles/panel", startingAppGraphNodeStylePanel);
            Set("theme_override_styles/titlebar", startingAppGraphNodeStyleTitlebar);
        }

        /// <summary>
        /// Initializes the node's UI and emits the ready signal.
        /// </summary>
        public override void _ReadyNode()
        {
            HBoxContainer mainContainer = GodotHelpers.CreateSlotWithDescription(AddonConstants.GraphNode.StartAppGraphNode.Color, Enums.SlotMode.Out, AddonConstants.GraphNode.StartAppGraphNode.descriptionLabelText);

            AddChild(mainContainer);
            SetSlot(mainContainer.GetIndex(), false, 0, AddonConstants.GraphNode.StartAppGraphNode.Color, true, 0, AddonConstants.GraphNode.StartAppGraphNode.Color);
            EmitSignal(SignalName.GraphNodeReady);
        }
    }
}
#endif