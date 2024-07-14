using System;
using Godot;
using MoF.Addons.ScenesManager.Constants;
using MoF.Addons.ScenesManager.Scripts.Resources;

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

        public static FileDialog CreateFileDialog(FileDialog.FileModeEnum mode, string title, Action<FileDialog, string, FileDialog.FileModeEnum> onFileSelected, Control parent)
        {
            FileDialog fileDialog = new FileDialog
            {
                FileMode = mode,
                Access = FileDialog.AccessEnum.Filesystem,
                ModeOverridesTitle = true,
                Title = title,
                Filters = new[] { "*.tres" },
                Size = new Vector2I((int)parent.GetViewport().GetVisibleRect().Size.X / 3, (int)parent.GetViewport().GetVisibleRect().Size.Y / 3),
            };

            fileDialog.FileSelected += (string path) => onFileSelected(fileDialog, path, mode);
            parent.AddChild(fileDialog);
            fileDialog.PopupCentered();
            return fileDialog;
        }

        public static ScenesManagerBaseGraphNode CreateGraphNodeFromItem(SceneManagerBaseItem item)
        {

            ScenesManagerBaseGraphNode node = null;
            if (item is SceneManagerItem sceneManagerItem)
            {
                node = new SceneGraphNode { Scene = sceneManagerItem.Scene, Name = sceneManagerItem.Name };
            }
            else if (item is StartAppSceneManagerItem startAppSceneManagerItem)
            {
                node = new StartAppGraphNode { Name = startAppSceneManagerItem.Name };
            }
            else
            {
                GD.PrintErr("Unknown SceneManagerItem type: " + item.GetType().Name);
            }
            return node;
        }

        public static void SaveSceneManagerSettings(string path)
        {
            SceneManagerSettings sceneManagerSettings = new()
            {
                SceneManagerSchemaPath = path,
            };
            ResourceSaver.Save(sceneManagerSettings, AddonConstants.SettingsFilePath);
        }
    }
}