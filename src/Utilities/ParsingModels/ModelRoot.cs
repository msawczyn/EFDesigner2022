using System.Collections.Generic;

namespace ParsingModels
{
   /// <summary>
   ///    Represents the root element of the XML model.
   /// </summary>
   public class ModelRoot
   {
      /// <summary>
      ///    Constructor for ModelRoot class.
      /// </summary>
      public ModelRoot()
      {
         Classes = new List<ModelClass>();
         Enumerations = new List<ModelEnum>();
      }

      /// <summary>
      ///    Gets or sets the name of the entity container.
      /// </summary>
      public string EntityContainerName { get; set; }

      /// <summary>
      ///    Gets or sets the namespace of the element.
      /// </summary>
      public string Namespace { get; set; }

      /// <summary>
      ///    Gets the fully qualified name (FQN) of the element.
      /// </summary>
      public string FullName
      {
         get
         {
            return string.IsNullOrWhiteSpace(Namespace)
                      ? $"global::{EntityContainerName}"
                      : $"global::{Namespace}.{EntityContainerName}";
         }
      }

      /// <summary>
      ///    Gets or sets the list of ModelClass instances.
      /// </summary>
      public List<ModelClass> Classes { get; set; }

      /// <summary>
      ///    Gets or sets the enumerations list.
      /// </summary>
      public List<ModelEnum> Enumerations { get; set; }
   }
}