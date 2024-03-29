﻿using System;

using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Diagrams;

namespace Sawczyn.EFDesigner.EFModel.Extensions
{
   /// <summary>
   ///    Extension methods for Microsoft.VisualStudio.Modeling.Diagrams.DiagramView
   /// </summary>
   public static class DiagramViewExtensions
   {
      /// <summary>
      ///    Sets the selection state of a shape element that represents the modelElement parameter on the given DiagramView
      /// </summary>
      /// <param name="diagramView">Object containing the shape element to select</param>
      /// <param name="modelElement">Model element the shape represents</param>
      /// <returns>True if the shape is present, visible and now selected, false otherwise</returns>
      public static bool SelectModelElement(this DiagramView diagramView, ModelElement modelElement)
      {
         // Get the shape element that corresponds to the model element

         ShapeElement shapeElement = modelElement.GetFirstShapeElement();

         if (shapeElement != null)
         {
            // Make sure the shape element is visible (because connectors can be hidden)

            if (!shapeElement.IsVisible)
               shapeElement.Show();

            // Create a diagram item for this shape element and select it
            diagramView.Selection.Set(new DiagramItem(shapeElement));

            return true;
         }

         // If the model element does not have a shape, try to cast it IModelElementCompartmented

         if (modelElement is IModelElementInCompartment compartmentedModelElement && compartmentedModelElement.ParentModelElement is ModelElement parentModelElement)
         {
            // Get the compartment that stores the model element

            ElementListCompartment compartment = parentModelElement.GetCompartment(compartmentedModelElement.CompartmentName);

            if (compartment == null)
               throw new InvalidOperationException($"Can't find compartment {compartmentedModelElement.CompartmentName}");

            // Expand the compartment
            if (!compartment.IsExpanded)
            {
               using (Transaction trans = modelElement.Store.TransactionManager.BeginTransaction("IsExpanded"))
               {
                  compartment.IsExpanded = true;
                  trans.Commit();
               }
            }

            // Find the model element in the compartment

            int index = compartment.Items.IndexOf(modelElement);

            if (index >= 0)
            {
               // Create a diagram item and select it

               diagramView.Selection.Set(new DiagramItem(compartment, compartment.ListField, new ListItemSubField(index)));

               return true;
            }
         }

         return false;
      }
   }
}