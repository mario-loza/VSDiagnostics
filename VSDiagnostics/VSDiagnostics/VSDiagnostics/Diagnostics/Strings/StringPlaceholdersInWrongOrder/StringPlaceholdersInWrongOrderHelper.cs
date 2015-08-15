﻿using System.Text.RegularExpressions;

namespace VSDiagnostics.Diagnostics.Strings.StringPlaceholdersInWrongOrder
{
    internal static class StringPlaceholdersInWrongOrderHelper
    {
        /// <summary>
        ///     Removes all curly braces and formatting definitions from the placeholder
        /// </summary>
        /// <param name="input">The placeholder entry to parse.</param>
        /// <returns>Returns the placeholder index.</returns>
        internal static string Normalize(string input)
        {
            var temp = input.Trim('{', '}');
            var colonIndex = temp.IndexOf(':');
            if (colonIndex > 0)
            {
                return temp.Remove(colonIndex);
            }

            return temp;
        }

        /// <summary>
        ///     Get all elements in a string that are enclosed by an uneven amount of curly brackets (to account for escaped
        ///     brackets).
        ///     The result will be elements that are either plain integers or integers with a format appended to it, delimited by a
        ///     colon.
        /// </summary>
        /// <param name="input">The format string with placeholders.</param>
        /// <returns>Returns a collection of matches according to the regex.</returns>
        internal static MatchCollection GetPlaceholders(string input)
        {
            var pattern = @"(?<!\{)\{(?:\{\{)*(\d+(?::.*?)?)\}(?:\}\})*(?!\})";
            return Regex.Matches(input, pattern);
        }

        internal static string[] GetPlaceholdersSplit(string input)
        {
            var pattern = @"(?<!\{)\{(?:\{\{)*(\d+(?::.*?)?)\}(?:\}\})*(?!\})";
            return Regex.Split(input, pattern);
        }
    }
}