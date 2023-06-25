namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   ///    Extension methods for System.String
   /// </summary>
   public static class StringExtensions
   {
      /// <summary>
      /// Convert a string to CamelCase.
      /// </summary>
      /// <param name="s">The string to be converted.</param>
      /// <returns>The converted string in CamelCase format.</returns>
      public static string ToCamelCase(this string s)
      {
         return string.IsNullOrEmpty(s)
                   ? s
                   : $"{s.Substring(0, 1).ToLowerInvariant()}{(s.Length > 1 ? s.Substring(1) : string.Empty)}";
      }

      /// <summary>
      /// Truncates the given string to the specified length and appends the given ellipsis.
      /// </summary>
      /// <param name="text">The string to truncate.</param>
      /// <param name="length">The maximum length allowed for the given string.</param>
      /// <param name="ellipsis">The ellipsis to append to the truncated string.</param>
      /// <param name="keepFullWordAtEnd">A value that indicates if a full word should be kept at the end of the truncated string.</param>
      /// <returns>A truncated string with the given ellipsis appended.</returns>
      public static string Truncate(this string text, int length, string ellipsis = "...", bool keepFullWordAtEnd = true)
      {
         if (string.IsNullOrEmpty(text))
            return string.Empty;

         if (text.Length < length)
            return text;

         string result = text.TrimEnd().Substring(0, length);

         if (keepFullWordAtEnd && (result.IndexOf(' ') >= 0))
            result = result.Substring(0, result.LastIndexOf(' '));

         return result + ellipsis;
      }
   }
}