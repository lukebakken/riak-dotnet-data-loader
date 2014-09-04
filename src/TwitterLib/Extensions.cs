namespace TwitterLib
{
    using System;

    public static class Extensions
    {
        public static bool IsNullOrWhitespace(this string argString)
        {
            return String.IsNullOrWhiteSpace(argString);
        }
    }
}
