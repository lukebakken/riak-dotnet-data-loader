namespace riak_data_loader
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
