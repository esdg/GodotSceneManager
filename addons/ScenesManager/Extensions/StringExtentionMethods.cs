namespace MoF.Addons.ScenesManager.Extensions
{
    public static partial class StringExtensionMethodsNode
    {
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