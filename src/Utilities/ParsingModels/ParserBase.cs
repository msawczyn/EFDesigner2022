using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ParsingModels
{
   public abstract class ParserBase
   {
      protected static readonly Regex TypeNameRegex = new Regex(@"([^`]+)`\d\[(\[[^\]]+\])(,(\[[^\]]+\]))*\]", RegexOptions.Compiled);

      protected static readonly List<string> IgnoreAttributes = new List<string>(new[]
                                                                                 {
                                                                                    "System.SerializableAttribute"
                                                                                  , "System.Runtime.InteropServices.ComVisibleAttribute"
                                                                                  , "__DynamicallyInvokableAttribute"
                                                                                  , "System.Reflection.DefaultMemberAttribute"
                                                                                  , "System.Runtime.Versioning.NonVersionableAttribute"
                                                                                  , "System.FlagsAttribute"
                                                                                  , "TableAttribute("
                                                                                  , "IsReadOnlyAttribute("
                                                                                  , "NullableAttribute("
                                                                                  , "NullableContextAttribute("
                                                                                 });

      protected readonly Logger log;

      private readonly Regex varcharPattern = new Regex(@"varchar\((.+)\)"
                                                      , RegexOptions.CultureInvariant
                                                      | RegexOptions.IgnoreCase
                                                      | RegexOptions.IgnorePatternWhitespace
                                                      | RegexOptions.Compiled);

      protected ParserBase(Logger logger)
      {
         log = logger;
      }

      protected static Multiplicity ConvertMultiplicity(RelationshipMultiplicity relationshipMultiplicity)
      {
         Multiplicity multiplicity = Multiplicity.ZeroOne;

         switch (relationshipMultiplicity)
         {
            case RelationshipMultiplicity.ZeroOrOne:
               multiplicity = Multiplicity.ZeroOne;

               break;

            case RelationshipMultiplicity.One:
               multiplicity = Multiplicity.One;

               break;

            case RelationshipMultiplicity.Many:
               multiplicity = Multiplicity.ZeroMany;

               break;
         }

         return multiplicity;
      }

      protected static string GetCustomAttributes(Type type)
      {
         return type == null
                   ? string.Empty
                   : GetCustomAttributes(type.CustomAttributes);
      }

      protected static string GetCustomAttributes(IEnumerable<CustomAttributeData> customAttributeData)
      {
         List<string> customAttributes = customAttributeData.Select(a => a.ToString()).ToList();
         customAttributes.RemoveAll(s => IgnoreAttributes.Select(s.Contains).Any());

         return string.Join("", customAttributes);
      }

      protected static string GetTypeFullName(Type type)
      {
         return GetTypeFullName(type?.FullName);
      }

      protected static string GetTypeFullName(string fullName)
      {
         if (string.IsNullOrWhiteSpace(fullName))
            return null;

         Match m = TypeNameRegex.Match(fullName);

         if (m.Success)
         {
            List<string> typeNames = new List<string>();
            string baseName = m.Groups[1].Value;
            typeNames.Add(m.Groups[2].Value.Trim('[', ']').Split(',')[0]);

            if (m.Groups.Count > 2)
            {
               foreach (Capture capture in m.Groups[3].Captures)
                  typeNames.Add(capture.Value.Trim(',', '[', ']').Split(',')[0]);
            }

            return $"{baseName}<{string.Join(",", typeNames)}>";
         }

         return fullName;
      }

      protected int ParseVarcharTypeAttribute(List<CustomAttributeData> attributes)
      {
         List<CustomAttributeData> typeNameAttributes = attributes.Where(a => a.AttributeType.Name == "TypeNameAttribute").ToList();

         foreach (CustomAttributeData attributeData in typeNameAttributes)
         {
            List<CustomAttributeTypedArgument> stringArguments = attributeData.ConstructorArguments
                                                                              .Where(x => x.Value is string)
                                                                              .ToList();

            foreach (Match match in stringArguments.Select(argument => varcharPattern.Match(argument.Value.ToString())))
            {
               if (match.Success && int.TryParse(match.Groups[1].ToString(), out int width))
               {
                  attributes.Remove(attributeData);

                  return width;
               }
            }
         }

         return 0;
      }
   }
}