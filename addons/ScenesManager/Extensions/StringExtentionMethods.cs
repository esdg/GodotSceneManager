namespace MoF.Addons.ScenesManager.Extensions
{
    /// <summary>
    /// Provides extension methods for working with strings.
    /// </summary>
    public static partial class StringExtensionMethodsNode
    {
        /// <summary>
        /// Capitalizes the first letter of each word in the input string.
        /// </summary>
        /// <param name="input">The input string to capitalize.</param>
        /// <returns>
        /// A string with the first letter of each word capitalized and the rest of the letters in lowercase.
        /// </returns>
        public static string CapitalizeFirstLetterOfEachWord(this string input)
        {
            // Split the string into words
            string[] words = input.Split(' ');

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    // Capitalize the first letter and make the rest lowercase
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
                }
            }

            // Join the words back into a single string
            return string.Join(' ', words);
        }
    }
}
