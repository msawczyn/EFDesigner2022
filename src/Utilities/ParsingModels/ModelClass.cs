using System.Collections.Generic;
using System.Diagnostics;

namespace ParsingModels
{
   [DebuggerDisplay("{FullName}")]
   public class ModelClass
   {
      public ModelClass()
      {
         Properties = new List<ModelProperty>();
      }

      public string FullName
      {
         get
         {
            return string.IsNullOrWhiteSpace(Namespace)
                      ? $"global::{Name}"
                      : $"global::{Namespace}.{Name}";
         }
      }

      public string Name { get; set; }
      public string Namespace { get; set; }
      public string CustomAttributes { get; set; }
      public string CustomInterfaces { get; set; }
      public bool IsAbstract { get; set; }
      public string BaseClass { get; set; }
      public string TableName { get; set; }
      public string ViewName { get; set; }
      public bool IsDependentType { get; set; }
      public bool IsPersistent { get; set; } = true;
      public List<ModelProperty> Properties { get; set; }
      public string Summary { get; set; }
      public List<ModelUnidirectionalAssociation> UnidirectionalAssociations { get; set; } = new List<ModelUnidirectionalAssociation>();
      public List<ModelBidirectionalAssociation> BidirectionalAssociations { get; set; } = new List<ModelBidirectionalAssociation>();
   }
}