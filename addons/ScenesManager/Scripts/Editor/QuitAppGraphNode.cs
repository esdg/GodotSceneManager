#if TOOLS
using Godot;
using Godot.Collections;
using MoF.Addons.ScenesManager.Constants;
using MoF.Addons.ScenesManager.Helpers;

namespace MoF.Addons.ScenesManager.Scripts.Editor
{
    /// <summary>
    /// Represents a custom graph node for quitting the application in the Scenes Manager editor.
    /// </summary>
    [Tool]
    public partial class QuitAppGraphNode : ScenesManagerBaseGraphNode
    {
        /// <summary>
        /// Cached texture for the quit app icon.
        /// </summary>
        private static Texture2D quitAppIconTexture;

        /// <summary>
        /// Cached style for the graph node panel.
        /// </summary>
        private static StyleBoxFlat quitAppGraphNodeStylePanel;

        /// <summary>
        /// Cached style for the graph node titlebar.
        /// </summary>
        private static StyleBoxFlat quitAppGraphNodeStyleTitlebar;

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
                var signals = new Array<string> { "Quit App Out slot" };
                return signals;
            }
        }

        /// <summary>
        /// Loads required resources for the node, such as textures and styles.
        /// </summary>
        public override void _LoadResources()
        {
            quitAppIconTexture ??= ResourceLoader.Load<Texture2D>(Plugin.PathToPlugin + AddonConstants.GraphNode.QuitAppGraphNode.StartingAppIconTexturePath);
            quitAppGraphNodeStylePanel ??= ResourceLoader.Load<StyleBoxFlat>(Plugin.PathToPlugin + AddonConstants.GraphNode.QuitAppGraphNode.GraphNodeStylePanelPath);
            quitAppGraphNodeStyleTitlebar ??= ResourceLoader.Load<StyleBoxFlat>(Plugin.PathToPlugin + AddonConstants.GraphNode.QuitAppGraphNode.GraphNodeStyleTitlebarPath);
        }

        /// <summary>
        /// Sets up the graph node's appearance and properties.
        /// </summary>
        public override void _SetupGraphNode()
        {
            Title = AddonConstants.GraphNode.QuitAppGraphNode.Title;
            Size = AddonConstants.GraphNode.QuitAppGraphNode.InitialSize;
            Set("theme_override_constants/separation", AddonConstants.GraphNode.NodeVerticalSpace);
            Set("theme_override_styles/panel", quitAppGraphNodeStylePanel);
            Set("theme_override_styles/titlebar", quitAppGraphNodeStyleTitlebar);
        }

        /// <summary>
        /// Initializes the node's UI and emits the ready signal.
        /// </summary>
        public override void _ReadyNode()
        {
            HBoxContainer mainContainer = GodotHelpers.CreateSlotWithDescription(AddonConstants.GraphNode.QuitAppGraphNode.Color, Enums.SlotMode.In, AddonConstants.GraphNode.QuitAppGraphNode.descriptionLabelText);

            AddChild(mainContainer);
            SetSlot(mainContainer.GetIndex(), true, 0, AddonConstants.GraphNode.QuitAppGraphNode.Color, false, 0, AddonConstants.GraphNode.QuitAppGraphNode.Color);
            EmitSignal(SignalName.GraphNodeReady);
        }
    }
}
#endif