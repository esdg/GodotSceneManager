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
        }

        public static void CreateNodesMenu(MenuBar mainMenuBar, PopupMenu.IndexPressedEventHandler eventHandler)
        {
            var nodesAddSubMenuItems = new[]
            {
                "Add Scene Node",
                "Add Transition Node"
            };

            var menuItemNodes = CreatePopupMenu("Nodes", Array.Empty<string>());
            var nodesAddSubMenuItem = CreatePopupMenu("NodesAddSubMenu", nodesAddSubMenuItems, eventHandler);

            menuItemNodes.AddSubmenuItem("Add", nodesAddSubMenuItem.Name);
            menuItemNodes.AddChild(nodesAddSubMenuItem);
            mainMenuBar.AddChild(menuItemNodes);
        }

        private static PopupMenu CreatePopupMenu(string name, string[] items, PopupMenu.IndexPressedEventHandler indexPressedHandler = null)
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

            if (indexPressedHandler != null)
            {
                menu.IndexPressed += indexPressedHandler;
            }

            return menu;
        }
    }
}
