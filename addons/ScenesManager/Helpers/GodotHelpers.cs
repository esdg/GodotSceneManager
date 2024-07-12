using Godot;
using MoF.Addons.ScenesManager.Constants;

namespace MoF.Addons.ScenesManager.Helpers
{
    public static class GodotHelpers
    {
        public static PopupMenu CreatePopupMenu(string name, string[] items, PopupMenu.IndexPressedEventHandler indexPressedHandler = null)
        {
            int itemIndex = 0;
            var menu = new PopupMenu { Name = name };
            foreach (var item in items)
            {
                if (item == AddonConstants.PopupMenuSeparator)
                {
                    menu.AddSeparator();
                }
                else
                {
                    menu.AddItem(item, itemIndex);
                    itemIndex++;
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