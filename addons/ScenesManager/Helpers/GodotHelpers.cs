using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Godot;
using MoF.Addons.ScenesManager.Constants;
using MoF.Addons.ScenesManager.Extensions;
using MoF.Addons.ScenesManager.Scripts.Resources;
using static Godot.Control;

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

        public static OptionButton CreateOptionButton()
        {
            return new OptionButton
            {
                TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis,
                FitToLongestItem = false,
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
        }

        public static string ToReadableFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return fileName;
            }

            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            // Replace underscores and hyphens with spaces in the name without extension
            string readableName = Regex.Replace(nameWithoutExtension, "[-_]", " ");

            // Insert spaces before uppercase letters (handling CamelCase)
            readableName = Regex.Replace(readableName, "(?<!^)([A-Z])", " $1");

            // Trim any extra spaces that might result from replacements
            readableName = readableName.Trim();

            // Ensure single spaces between words
            readableName = Regex.Replace(readableName, @"\s+", " ");

            // Capitalize the first letter of each word
            readableName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(readableName.ToLower());

            return readableName;
        }
    }
}