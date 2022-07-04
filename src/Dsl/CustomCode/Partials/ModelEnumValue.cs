using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Diagrams;
using Microsoft.VisualStudio.Modeling.Validation;

using Sawczyn.EFDesigner.EFModel.Annotations;
using Sawczyn.EFDesigner.EFModel.Extensions;

namespace Sawczyn.EFDesigner.EFModel
{
   [ValidationState(ValidationState.Enabled)]
   public partial class ModelEnumValue : IModelElementInCompartment, IDisplaysWarning, IHasStore
   {
      private ModelEnum cachedParent;

      public IModelElementWithCompartments ParentModelElement
      {
         get
         {
            return Enum;
         }
      }

      public string CompartmentName
      {
         get
         {
            return this.GetFirstShapeElement().AccessibleName;
         }
      }

      public string GetDisplayText()
      {
         return $"{Enum.Name}.{Name}";
      }

      /// <summary>
      ///    Called by the model after the element has been deleted.
      /// </summary>
      protected override void OnDeleted()
      {
         base.OnDeleted();

         cachedParent?.SetFlagValues();
      }

      /// <summary>Called by the model before the element is deleted.</summary>
      protected override void OnDeleting()
      {
         base.OnDeleting();

         cachedParent = Enum;
      }

      [ValidationMethod(ValidationCategories.Open | ValidationCategories.Save | ValidationCategories.Menu)]
      [UsedImplicitly]
      [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by validation")]
      private void SummaryDescriptionIsEmpty(ValidationContext context)
      {
         if (Enum?.ModelRoot == null)
            return;

         ModelRoot modelRoot = Store.ElementDirectory.FindElements<ModelRoot>().FirstOrDefault();

         if ((Enum != null) && (modelRoot?.WarnOnMissingDocumentation == true) && string.IsNullOrWhiteSpace(Summary))
         {
            context.LogWarning($"{Enum.Name}.{Name}: Enum value should be documented", "AWMissingSummary", this);
            hasWarning = true;
            RedrawItem();
         }
      }

      /// <summary>Returns a string that represents the current object.</summary>
      /// <returns>A string that represents the current object.</returns>
      public override string ToString()
      {
         return Name
              + (string.IsNullOrEmpty(Value)
                    ? string.Empty
                    : $" = {Value}");
      }

#region Warning display

      // set as methods to avoid issues around serialization

      private bool hasWarning;

      public bool GetHasWarningValue()
      {
         return hasWarning;
      }

      public void ResetWarning()
      {
         hasWarning = false;
      }

      public void RedrawItem()
      {
         // redraw on every diagram
         foreach (ShapeElement shapeElement in
                  PresentationViewsSubject.GetPresentation(ParentModelElement as ModelElement).OfType<ShapeElement>().Distinct())
            shapeElement.Invalidate();
      }

#endregion
   }
}