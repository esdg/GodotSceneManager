using System;
using Godot;
using MoF.Addons.ScenesManager.Constants;
using MoF.Addons.ScenesManager.Scripts.Resources;

namespace MoF.Addons.ScenesManager.Helpers
{
    public static class GodotHelpers
    {
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
                node = new SceneGraphNode { Scene = sceneManagerItem.Scene, GraphNodeName = sceneManagerItem.Name };
            }
            else if (item is StartAppSceneManagerItem startAppSceneManagerItem)
            {
                node = new StartAppGraphNode { GraphNodeName = startAppSceneManagerItem.Name };
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