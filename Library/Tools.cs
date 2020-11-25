using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Freeze.Library
{
    public static class FreezeTools
    {
        private static readonly Regex emptyLine = new Regex("^\\s*$", RegexOptions.Compiled);
        public static IEnumerable<string> Get(this string collection, string header, string endHeader = "", bool untilEnd = false)
        {
            var a = collection.Split(Environment.NewLine)
                .SkipWhile(s => !Regex.IsMatch(s, header))
                .SkipWhile(s => emptyLine.IsMatch(s));

            if (untilEnd)
                return a;
            else if (string.IsNullOrEmpty(endHeader))
                return a.Skip(1).TakeWhile(s => !emptyLine.IsMatch(s));
            else
                return a.TakeWhile(s => !Regex.IsMatch(s, endHeader));
        }

        public static string ConvertToStatus(bool enabled)
        {
            return enabled ? "Active" : "Inactive";
        }
    }
}