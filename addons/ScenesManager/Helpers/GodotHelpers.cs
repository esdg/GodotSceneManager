using System;
using System.IO;
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

        public static void SaveSceneManagerSettings(string path)
        {
            SceneManagerSettings sceneManagerSettings = new()
            {
                SceneManagerSchemaPath = path,
            };
            ResourceSaver.Save(sceneManagerSettings, AddonConstants.SettingsFilePath);
        }

        public static string GetSceneGraphNodeTitle(Node node)
        {
            return $"{Path.GetFileNameWithoutExtension(node.SceneFilePath)}::{node.Name}";
        }

    }
}