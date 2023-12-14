using System;
using System.Linq;

namespace Sawczyn.EFDesigner.EFModel
{
   public partial class EFModelDomainModel
   {
      internal static readonly Type[] RuleClasses =
      {
         typeof(AssociationAddRules),
         typeof(AssociationChangedRules),
         typeof(AssociationDeletingRules),
         typeof(GeneralizationAddRules),
         typeof(GeneralizationChangeRules),
         typeof(GeneralizationDeletingRules),
         typeof(ModelAttributeAddRules),
         typeof(ModelAttributeChangeRules),
         typeof(ModelClassAddRules),
         typeof(ModelClassChangeRules),
         typeof(ModelClassDeletingRules),
         typeof(ModelDiagramDataAddRules),
         typeof(ModelDiagramDataChangeRules),
         typeof(ModelDiagramDataDeleteRules),
         typeof(ModelEnumAddRules),
         typeof(ModelEnumChangeRules),
         typeof(ModelEnumDeletingRules),
         typeof(ModelEnumValueAddRules),
         typeof(ModelEnumValueChangeRules),
         typeof(ModelRootChangeRules)
      };

      /// <summary>Gets the list of non-generated domain model types.</summary>
      /// <returns>List of types.</returns>
      protected override Type[] GetCustomDomainModelTypes()
      {
         return base.GetCustomDomainModelTypes().Concat(RuleClasses).ToArray();
      }
   }
}