using System.Diagnostics;

namespace ParsingModels
{
   /// <summary>
   ///    Represents a value of a ModelEnum.
   /// </summary>
   [DebuggerDisplay("{Name} = {Value")]
   public class ModelEnumValue
   {
      /// <summary>
      ///    Gets or sets the name.
      /// </summary>
      public string Name { get; set; }

      /// <summary>
      ///    Gets or sets the string value.
      /// </summary>
      public string Value { get; set; }

      /// <summary>
      ///    Gets or sets the custom attributes string.
      /// </summary>
      public string CustomAttributes { get; set; }

      /// <summary>
      ///    Gets or sets the text to be displayed.
      /// </summary>
      public string DisplayText { get; set; }
   }
}