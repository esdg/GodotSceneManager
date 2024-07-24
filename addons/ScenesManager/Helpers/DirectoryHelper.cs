using System.Text.RegularExpressions;
using Godot;
using Godot.Collections;

namespace MoF.Addons.ScenesManager.Helpers
{
    /// <summary>
    /// Provides helper methods for file system operations.
    /// </summary>
    public static partial class FileSystemHelper
    {
        /// <summary>
        /// Retrieves the content of a directory.
        /// </summary>
        /// <param name="path">The path of the directory.</param>
        /// <param name="recursive">If set to <c>true</c>, retrieves content recursively.</param>
        /// <param name="extension">The file extension filter. Default is "*.*".</param>
        /// <returns>
        /// An array of file paths matching the specified extension.
        /// </returns>
        public static Array<string> DirContent(string path, bool recursive = false, string extension = "*.*")
        {
            if (!path.EndsWith("/"))
            {
                path += "/";
            }

            using var dir = DirAccess.Open(path);
            if (dir == null)
            {
                GD.PrintErr($"An error occurred when trying to access the path: {path}.");
                return null;
            }

            Array<string> items = new();
            GD.Print($"Fetching resources in path: {path}.");
            dir.ListDirBegin();

            string fileName = dir.GetNext();
            while (fileName != "")
            {
                if (fileName == "." || fileName == "..")
                {
                    fileName = dir.GetNext();
                    continue;
                }

                string fullPath = path + fileName;

                if (dir.CurrentIsDir())
                {
                    if (recursive)
                    {
                        GD.Print($"Found folder: {fullPath}/");
                        items.AddRange(DirContent(fullPath, true, extension));
                    }
                }
                else if (FileNameMatchesPattern(fileName, extension))
                {
                    items.Add(fullPath);
                    GD.Print($"Found file: {fullPath}.");
                }

                fileName = dir.GetNext();
            }

            return items;
        }

        /// <summary>
        /// Retrieves the scenes from a directory.
        /// </summary>
        /// <typeparam name="T">The type of scenes to retrieve.</typeparam>
        /// <param name="path">The path of the directory.</param>
        /// <param name="recursive">If set to <c>true</c>, retrieves scenes recursively.</param>
        /// <param name="extension">The file extension filter. Default is "*.*".</param>
        /// <returns>
        /// An array of scene paths matching the specified extension.
        /// </returns>
        public static Array<string> DirScenes<T>(string path, bool recursive = false, string extension = "*.*")
        {
            return DirContent(path, recursive, extension);
        }

        /// <summary>
        /// Checks if a filename matches a specified pattern.
        /// </summary>
        /// <param name="filename">The filename to check.</param>
        /// <param name="pattern">The pattern to match against.</param>
        /// <returns>
        /// <c>true</c> if the filename matches the pattern; otherwise, <c>false</c>.
        /// </returns>
        public static bool FileNameMatchesPattern(string filename, string pattern)
        {
            // Escape any regex special characters except for * and .
            string regexPattern = Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".");

            // Add start and end anchors
            regexPattern = "^" + regexPattern + "$";

            // Create regex
            Regex regex = new(regexPattern, RegexOptions.IgnoreCase);

            // Match the filename against the regex pattern
            return regex.IsMatch(filename);
        }

        /// <summary>
        /// Saves a resource and creates the folder if it doesn't exist.
        /// </summary>
        /// <param name="resource">The resource to save.</param>
        /// <param name="path">The path where the resource will be saved.</param>
        public static void SaveAndCreateFolder(Resource resource, string path)
        {
            // Ensure the directory exists using Godot's Directory class
            string directoryPath = path.GetBaseDir();
            if (!DirAccess.DirExistsAbsolute(directoryPath))
            {
                DirAccess.MakeDirRecursiveAbsolute(directoryPath);
            }
            ResourceSaver.Save(resource, Plugin.PathToPlugin + path);
        }
    }
}
