#if TOOLS
using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Godot;
using MoF.Addons.ScenesManager.Constants;
using MoF.Addons.ScenesManager.Scripts.Resources;
using static Godot.Control;


namespace MoF.Addons.ScenesManager.Helpers
{
    /// <summary>
    /// Provides helper methods for common Godot-related tasks.
    /// </summary>
    public static class GodotHelpers
    {
        /// <summary>
        /// Creates a <see cref="FileDialog"/> with specified settings.
        /// </summary>
        /// <param name="mode">The file dialog mode.</param>
        /// <param name="title">The title of the file dialog.</param>
        /// <param name="onFileSelected">The action to execute when a file is selected.</param>
        /// <param name="parent">The parent control to which the file dialog will be added.</param>
        /// <returns>The created <see cref="FileDialog"/>.</returns>
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

        /// <summary>
        /// Saves the scene manager settings, with path to the scene manager schema file.
        /// </summary>
        /// <param name="path">The path to the scene manager schema file.</param>
        public static void SaveSceneManagerSettings(string path)
        {
            SceneManagerSettings sceneManagerSettings = new()
            {
                SceneManagerSchemaPath = path,
            };
            FileSystemHelper.SaveAndCreateFolder(sceneManagerSettings, AddonConstants.SettingsFilePath);
        }

        /// <summary>
        /// Gets the title of a scene graph node.
        /// </summary>
        /// <param name="node">The node for which to get the title.</param>
        /// <returns>The title of the scene graph node.</returns>
        public static string GetSceneGraphNodeTitle(Node node)
        {
            return $"{Path.GetFileNameWithoutExtension(node.SceneFilePath)}::{node.Name}";
        }

        /// <summary>
        /// Creates a configured <see cref="OptionButton"/>.
        /// </summary>
        /// <returns>The created <see cref="OptionButton"/>.</returns>
        public static OptionButton CreateOptionButton()
        {
            return new OptionButton
            {
                TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis,
                FitToLongestItem = false,
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
        }

        /// <summary>
        /// Converts a file name to a more readable format.
        /// </summary>
        /// <param name="fileName">The file name to convert.</param>
        /// <returns>A readable version of the file name.</returns>
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
#endif