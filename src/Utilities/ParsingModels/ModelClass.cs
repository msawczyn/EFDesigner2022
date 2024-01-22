using System.Collections.Generic;
using System.Diagnostics;

namespace ParsingModels
{
   /// <summary>
   ///    Represents a model class.
   /// </summary>
   [DebuggerDisplay("{FullName}")]
   public class ModelClass
   {
      /// <summary>
      ///    Initializes a new instance of the <see cref="ModelClass" /> class.
      /// </summary>
      public ModelClass()
      {
         Properties = new List<ModelProperty>();
      }

      /// <summary>
      ///   Gets or sets a value indicating whether to generate code for the model class. May be false if the class is defined in a referenced assembly.
      /// </summary>
      public bool GenerateCode { get; set; } = true;

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

      /// <summary>
      ///    Gets or sets the name.
      /// </summary>
      public string Name { get; set; }

      /// <summary>
      ///    Gets or sets the namespace of the code element.
      /// </summary>
      public string Namespace { get; set; }

      /// <summary>
      ///    Gets or sets the custom attributes.
      /// </summary>
      public string CustomAttributes { get; set; }

      /// <summary>
      ///    Gets or sets the custom interfaces.
      /// </summary>
      public string CustomInterfaces { get; set; }

      /// <summary>
      ///    Gets or sets a value indicating whether this instance is abstract.
      /// </summary>
      public bool IsAbstract { get; set; }

      /// <summary>
      ///    Gets or sets the base class, if any.
      /// </summary>
      public string BaseClass { get; set; }

      /// <summary>
      ///    Gets or sets the name of the table, if any.
      /// </summary>
      public string TableName { get; set; }

      /// <summary>
      ///    Gets or sets the name of the view, if any.
      /// </summary>
      public string ViewName { get; set; }

      /// <summary>
      ///    Indicates whether the object is of a dependent type.
      /// </summary>
      public bool IsDependentType { get; set; }

      /// <summary>
      ///    Gets or sets a boolean value indicating whether the object is persistent or not.
      /// </summary>
      public bool IsPersistent { get; set; } = true;

      /// <summary>
      ///    Gets or sets the list of model properties.
      /// </summary>
      public List<ModelProperty> Properties { get; set; }

      /// <summary>
      ///    Gets or sets the summary of the object
      /// </summary>
      public string Summary { get; set; }

      /// <summary>
      ///    Gets or sets a list of unidirectional associations for the model class.
      /// </summary>
      public List<ModelUnidirectionalAssociation> UnidirectionalAssociations { get; set; } = new List<ModelUnidirectionalAssociation>();

      /// <summary>
      ///    Gets or sets the list of bidirectional associations for the model class.
      /// </summary>
      public List<ModelBidirectionalAssociation> BidirectionalAssociations { get; set; } = new List<ModelBidirectionalAssociation>();
   }
}