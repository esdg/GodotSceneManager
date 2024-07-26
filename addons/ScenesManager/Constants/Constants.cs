using Godot;

namespace MoF.Addons.ScenesManager.Constants
{
    /// <summary>
    /// A static class that holds constants for the Scenes Manager add-on.
    /// </summary>
    public static partial class AddonConstants
    {
        // General constants
        public const string PopupMenuSeparator = "------";
        public const string SettingsFilePath = "/Settings/SceneManagerSettings.tres";
        public const string TransitionFolderPath = "/TransitionsLibrary";
        public const string TransitionNone = "none";

        /// <summary>
        /// Constants related to graph nodes.
        /// </summary>
        public static partial class GraphNode
        {
            public const int NodeVerticalSpace = 6;
            public const int MaxNumberOfInSlots = 4;
            public const int MaxNumberOfOutSlots = 4;

            public static readonly Color InSlotColor = new("45F576");
            public static readonly Color OutSlotColor = new("FF5F5F");



            public static partial class Icons
            {
                public const string SignalIconTexture = "/Assets/Icons/SignalIcon.svg";
                public const string TrashcanIconTexture = "/Assets/Icons/trashcan.svg";
                public static readonly string[] TransitionIconsTexture = { "/Assets/Icons/TransitionIconOff.svg", "/Assets/Icons/TransitionIconOn.svg" };
            }


            /// <summary>
            /// Constants specific to scene graph nodes.
            /// </summary>
            public static partial class SceneGraphNode
            {
                public const string Title = "Scene Node";
                public const string GraphNodeStylePanelPath = "/Assets/Styles/SceneGraphNodeStylePanel.tres";
                public const string GraphNodeStyleTitlebarPath = "/Assets/Styles/SceneGraphNodeStyleTitlebar.tres";
                public static readonly Vector2 InitialSize = new(250, 10);
                public static readonly string[] linkImagesTransition = { "/Assets/Icons/LinkTransitionIconOff.svg", "/Assets/Icons/LinkTransitionIconOn.svg" };
            }

            /// <summary>
            /// Constants specific to starting app graph nodes.
            /// </summary>
            public static partial class StartAppGraphNode
            {
                public const string Title = "Starting App Node";
                public const string GraphNodeStylePanelPath = "/Assets/Styles/StartingAppGraphNodeStylePanel.tres";
                public const string GraphNodeStyleTitlebarPath = "/Assets/Styles/StartingAppGraphNodeStyleTitlebar.tres";
                public const string StartingAppIconTexturePath = "/Assets/Icons/starting-app-icon.svg";
                public static readonly Vector2 InitialSize = new(250, 10);
            }

            public static partial class QuitAppGraphNode
            {
                public const string Title = "Quit App Node";
                public const string GraphNodeStylePanelPath = "/Assets/Styles/QuitAppGraphNodeStylePanel.tres";
                public const string GraphNodeStyleTitlebarPath = "/Assets/Styles/QuitAppGraphNodeStyleTitlebar.tres";
                public const string StartingAppIconTexturePath = "/Assets/Icons/quit-app-icon.svg";
                public static readonly Vector2 InitialSize = new(250, 10);
            }
        }
    }
}
