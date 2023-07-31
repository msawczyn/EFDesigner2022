using System.Diagnostics;

namespace ParsingModels
{
   /// <summary>
   ///    Represents a single property of a data model
   /// </summary>
   [DebuggerDisplay("{Name}")]
   public class ModelProperty

   {
      /// <summary>
      ///    Gets or sets the name of the type.
      /// </summary>
      public string TypeName { get; set; }

      /// <summary>
      ///    Gets or sets the name.
      /// </summary>
      public string Name { get; set; }

      /// <summary>
      ///    Gets or sets the name of the column in the backing data store.
      /// </summary>
      public string ColumnName { get; set; }

      /// <summary>
      ///    Gets or sets the custom attributes.
      /// </summary>
      public string CustomAttributes { get; set; }

      /// <summary>
      ///    Gets or sets a value indicating whether the current item is indexed.
      /// </summary>
      public bool Indexed { get; set; }

      /// <summary>
      ///    Gets or sets a value indicating whether the property is indexed and unique.
      /// </summary>
      public bool IndexedUnique { get; set; }

      /// <summary>
      ///    Gets or sets the index name.
      /// </summary>
      public string IndexName { get; set; }

      /// <summary>
      ///    Gets or sets a value indicating whether the property is required or not.
      /// </summary>
      public bool Required { get; set; }

      /// <summary>
      ///    Gets or sets the maximum length of the string.
      /// </summary>
      public int MaxStringLength { get; set; }

      /// <summary>
      ///    Gets or sets the minimum length of a string.
      /// </summary>
      public int MinStringLength { get; set; }

      /// <summary>
      ///    Gets or sets a value indicating whether the current instance represents an identity.
      /// </summary>
      public bool IsIdentity { get; set; }

      /// <summary>
      ///    Gets or sets a value indicating whether the identity is generated.
      /// </summary>
      public bool IsIdentityGenerated { get; set; }

      /// <summary>
      ///    Gets or sets the summary of the object.
      /// </summary>
      public string Summary { get; set; }
   }
}