using System.Collections.Generic;
using System.Diagnostics;

namespace ParsingModels
{
   /// <summary>
   ///    Represents an enumeration for Model types.
   /// </summary>
   [DebuggerDisplay("{FullName}")]
   public class ModelEnum

   {
      /// <summary>
      ///    Constructor for the ModelEnum class
      /// </summary>
      public ModelEnum()
      {
         Values = new List<ModelEnumValue>();
      }

      /// <summary>
      ///    Gets or sets the name.
      /// </summary>
      public string Name { get; set; }

      /// <summary>
      ///    Gets or sets the namespace
      /// </summary>
      public string Namespace { get; set; }

      /// <summary>
      ///    Gets or sets the custom attributes.
      /// </summary>
      public string CustomAttributes { get; set; }

      /// <summary>
      ///    Gets or sets the list of model enum values.
      /// </summary>
      public List<ModelEnumValue> Values { get; set; }

      /// <summary>
      ///    Gets or sets the value type.
      /// </summary>
      public string ValueType { get; set; }

      /// <summary>
      ///    Gets or sets a value indicating whether this instance is flags.
      /// </summary>
      public bool IsFlags { get; set; }

      /// <summary>
      ///    Gets the full name of the person.
      /// </summary>
      public string FullName
      {
         get
         {
            return string.IsNullOrWhiteSpace(Namespace)
                      ? $"global::{Name}"
                      : $"global::{Namespace}.{Name}";
         }
      }
   }
}