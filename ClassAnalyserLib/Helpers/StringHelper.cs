namespace ClassAnalyser.Helpers
{
    public static class StringHelper
    {
        /// <summary>
        /// Determines whether the input string is null, empty or is a whitespace character.
        /// </summary>
        /// <param name="in_str">The string to check.</param>
        public static bool IsNullOrEmptyOrWhiteSpace(this string in_str)
        {
            return string.IsNullOrEmpty(in_str) || string.IsNullOrWhiteSpace(in_str);
        }
    }
}
