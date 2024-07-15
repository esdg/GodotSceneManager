using Godot;
using MoF.Addons.ScenesManager.Constants;
using System;
using System.Linq;

namespace MoF.Addons.ScenesManager.Helpers
{
    public static class MenuHelpers
    {
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

        public static void CreateNodesMenu(MenuBar mainMenuBar, PopupMenu.IndexPressedEventHandler eventHandler)
        {
            var nodesAddSubMenuItems = new[]
            {
                "Add Scene Node",
                "Add Transition Node",
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
