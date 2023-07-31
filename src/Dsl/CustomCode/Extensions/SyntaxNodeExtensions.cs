using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Sawczyn.EFDesigner.EFModel.Annotations;

namespace Sawczyn.EFDesigner.EFModel.Extensions
{
   /// <summary>
   ///    Extension methods for Microsoft.CodeAnalysis.SyntaxNode
   /// </summary>
   public static class SyntaxNodeExtensions
   {
      /// <summary>
      /// Checks whether or not a syntax node has the specified attribute.
      /// </summary>
      /// <param name="node">The syntax node to check.</param>
      /// <param name="attributeName">The name of the attribute to check for.</param>
      /// <returns>True if the syntax node has the specified attribute, otherwise false.</returns>
      public static bool HasAttribute([NotNull] this SyntaxNode node, string attributeName)
      {
         string fullname = attributeName.EndsWith("Attribute")
                              ? attributeName
                              : $"{attributeName}Attribute";

         string shortName = attributeName.EndsWith("Attribute")
                               ? attributeName.Substring(0, attributeName.Length - 9)
                               : attributeName;

         return node.DescendantNodes()
                    .OfType<AttributeSyntax>()
                    .Any(x => (x.Name.ToString() == shortName) || (x.Name.ToString() == fullname));
      }

#pragma warning disable CS3002 // Return type is not CLS-compliant
      /// <summary>
      /// Retrieves the attribute syntax of the specified name from the given syntax node.
      /// </summary>
      /// <param name="node">The syntax node to retrieve the attribute syntax from.</param>
      /// <param name="attributeName">The name of the attribute syntax to retrieve.</param>
      /// <returns>The attribute syntax with the specified name, null if the attribute syntax is not found.</returns>
      public static AttributeSyntax GetAttribute([NotNull] this SyntaxNode node, string attributeName)
      {
         string longName = attributeName.EndsWith("Attribute")
                              ? attributeName
                              : $"{attributeName}Attribute";

         string shortName = attributeName.EndsWith("Attribute")
                               ? attributeName.Substring(0, attributeName.Length - 9)
                               : attributeName;

         return node.DescendantNodes()
                    .OfType<AttributeSyntax>()
                    .FirstOrDefault(x => (x.Name.ToString() == shortName) || (x.Name.ToString() == longName));
      }

      /// <summary>
      /// Returns the argument list of an attribute.
      /// </summary>
      /// <param name="node">The attribute node to find argument list of.</param>
      /// <returns>An IEnumerable of AttributeArgumentSyntax representing the argument list of the attribute node.</returns>
      public static IEnumerable<AttributeArgumentSyntax> GetAttributeArguments([NotNull] this AttributeSyntax node)
      {
         return node.DescendantNodes().OfType<AttributeArgumentSyntax>();
      }

      /// <summary>
      /// Returns the value of a named argument in the given attribute syntax node.
      /// </summary>
      /// <param name="node">The attribute syntax node.</param>
      /// <param name="argumentName">The name of the named argument.</param>
      /// <returns>The value of the named argument.</returns>
      public static string GetNamedArgumentValue([NotNull] this AttributeSyntax node, string argumentName)
      {
         AttributeArgumentSyntax namedArgument =
            node.DescendantNodes()
                .OfType<AttributeArgumentSyntax>()
                .FirstOrDefault(aas => aas.DescendantNodes()
                                          .OfType<IdentifierNameSyntax>()
                                          .Any(ins => ins.Identifier.Text == argumentName));

         SyntaxToken? valueToken = namedArgument.DescendantNodes().OfType<LiteralExpressionSyntax>().FirstOrDefault()?.Token;

         return valueToken?.Text.Trim('"');
      }
#pragma warning restore CS3002 // Return type is not CLS-compliant
   }
}