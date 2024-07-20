using System.Text.RegularExpressions;
using Godot;
using Godot.Collections;

namespace MoF.Addons.ScenesManager.Helpers
{
    public static partial class DirectoryHelper
    {
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

        public static Array<string> DirScenes<T>(string path, bool recursive = false, string extension = "*.*")
        {
            return DirContent(path, recursive, extension);
        }

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
    }


}