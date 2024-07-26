#if TOOLS
using Godot;
using MoF.Addons.ScenesManager.Constants;
using System;

namespace MoF.Addons.ScenesManager.Helpers
{
    /// <summary>
    /// Provides helper methods for creating and managing menus in the application.
    /// </summary>
    public static class MenuHelpers
    {
        /// <summary>
        /// Creates a graph menu and adds it to the specified main menu bar.
        /// </summary>
        /// <param name="mainMenuBar">The main menu bar to which the graph menu will be added.</param>
        /// <param name="eventHandler">The event handler for handling index pressed events.</param>
        public static void CreateGraphMenu(MenuBar mainMenuBar, PopupMenu.IndexPressedEventHandler eventHandler)
        {
            var graphMenuItems = new[]
            {
                "New Graph",
                "Open Graph...",
                AddonConstants.PopupMenuSeparator,
                "Save Graph",
                "Save Graph As..."
            };

            var menuGraph = CreatePopupMenu("Graph", graphMenuItems, eventHandler);

            mainMenuBar.AddChild(menuGraph);
            menuGraph.Name = "Graph menu";
        }

        /// <summary>
        /// Creates a nodes menu and adds it to the specified main menu bar.
        /// </summary>
        /// <param name="mainMenuBar">The main menu bar to which the nodes menu will be added.</param>
        /// <param name="eventHandler">The event handler for handling index pressed events.</param>
        public static void CreateNodesMenu(MenuBar mainMenuBar, PopupMenu.IndexPressedEventHandler eventHandler)
        {
            var nodesAddSubMenuItems = new[]
            {
                "Add Scene Node",
            };

            var menuItemNodes = CreatePopupMenu("Nodes", Array.Empty<string>(), eventHandler);
            var nodesAddSubMenuItem = CreatePopupMenu("NodesAddSubMenu", nodesAddSubMenuItems, eventHandler);

            menuItemNodes.AddSubmenuItem("Add", nodesAddSubMenuItem.Name);
            menuItemNodes.AddChild(nodesAddSubMenuItem);
            menuItemNodes.AddSeparator();
            menuItemNodes.AddIconRadioCheckItem(null, "Quit Node");

            mainMenuBar.AddChild(menuItemNodes);
            menuItemNodes.Name = "Node menu";
        }

        /// <summary>
        /// Creates a popup menu with the specified name and items.
        /// </summary>
        /// <param name="name">The name of the popup menu.</param>
        /// <param name="items">The items to be added to the popup menu.</param>
        /// <param name="eventHandler">The event handler for handling index pressed events. Default is null.</param>
        /// <returns>The created <see cref="PopupMenu"/>.</returns>
        private static PopupMenu CreatePopupMenu(string name, string[] items, PopupMenu.IndexPressedEventHandler eventHandler = null)
        {
            var menu = new PopupMenu { Name = name };
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (item == AddonConstants.PopupMenuSeparator)
                {
                    menu.AddSeparator();
                }
                else
                {
                    menu.AddItem(item, i);
                }
            }

            if (eventHandler != null)
            {
                menu.IndexPressed += eventHandler;
            }

            return menu;
        }
    }
}
#endif