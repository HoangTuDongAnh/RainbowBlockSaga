using System.Text.RegularExpressions;

namespace RainbowBlockSaga.Presentation.Scripts.Utils
{
    public static class StringUtlis
    {
        public static string CamelCaseSplit(this string text)
        {
            return Regex.Replace(text, "((?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z]))", " $1").Trim();
        }

        public static int ToInt(this string text)
        {
            return int.Parse(text);
        }
    }
}