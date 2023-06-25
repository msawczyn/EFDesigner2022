using System;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   ///    Rules used in various places, consolidated here to remove technical debt stemming from copy/paste
   /// </summary>
   public static class CommonRules
   {
      /// <summary>
      /// Validates the namespace name using the provided validation function for language-independent identifier.
      /// </summary>
      /// <param name="ns">The name of the namespace that needs to be validated.</param>
      /// <param name="isValidLanguageIndependentIdentifier">The validation function for language-independent identifier.</param>
      /// <returns>Returns the valid namespace name.</returns>
      public static string ValidateNamespace(string ns, Func<string, bool> isValidLanguageIndependentIdentifier)
      {
         bool isBad = string.IsNullOrWhiteSpace(ns);

         if (!isBad)
         {
            string[] namespaceParts = ns.Split('.');

            foreach (string namespacePart in namespaceParts)
               isBad &= isValidLanguageIndependentIdentifier(namespacePart);
         }

         return isBad
                   ? "Namespace must exist and consist of valid .NET identifiers"
                   : null;
      }
   }
}