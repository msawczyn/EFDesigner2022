using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Diagrams;

namespace Sawczyn.EFDesigner.EFModel.Extensions
{
   /// <summary>
   ///    Extension methods for Microsoft.VisualStudio.Modeling.ModelElement
   /// </summary>
   public static class ModelElementExtensions
   {
      /// <summary>
      /// Returns the active diagram associated with the given ModelElement.
      /// </summary>
      /// <param name="element">The ModelElement to get the active diagram for.</param>
      /// <returns>The active diagram associated with the given ModelElement.</returns>
      public static Diagram GetActiveDiagram(this ModelElement element)
      {
         ShapeElement shapeElement = element.GetShapeElement();

         return shapeElement?.Diagram;
      }

      /// <summary>
      /// Retrieves all items of a certain type from the given store.
      /// </summary>
      /// <typeparam name="T">The type of the items to retrieve.</typeparam>
      /// <param name="store">The store from which to retrieve items.</param>
      /// <returns>An enumerable collection of items of the specified type.</returns>
      public static IEnumerable<T> GetAll<T>(this Store store) where T : ModelElement
      {
         return store?.ElementDirectory?.FindElements<T>(true) ?? new ReadOnlyCollection<T>(new List<T>());
      }

      ///<summary>
      ///Returns all the ModelElement objects of type T in the specified partition.
      ///</summary>
      ///<typeparam name="T">The type of the ModelElement object.</typeparam>
      ///<param name="partition">The partition that contains the ModelElement objects.</param>
      ///<returns>An IEnumerable of all the ModelElement objects of type T in the specified partition.</returns>
      public static IEnumerable<T> GetAll<T>(this Partition partition) where T : ModelElement
      {
         return partition?.ElementDirectory?.FindElements<T>(true) ?? new ReadOnlyCollection<T>(new List<T>());
      }

      /// <summary>
      ///    Gets the named compartment in this element
      /// </summary>
      /// <param name="element"></param>
      /// <param name="compartmentName"></param>
      /// <returns></returns>
      public static ElementListCompartment GetCompartment(this ModelElement element, string compartmentName)
      {
         return element?.GetFirstShapeElement()
                       ?.NestedChildShapes
                       ?.OfType<ElementListCompartment>()
                        .FirstOrDefault(s => s.Name == compartmentName);
      }

      /// <summary>
      ///    Finds the ModelElement surrounding the current ModelElement. Assumes current ModelElement is a compartment.
      /// </summary>
      /// <param name="modelElement">Compartment</param>
      /// <returns>Surrounding (owning) element, if any</returns>
      private static ModelElement GetCompartmentElementFirstParentElement(this ModelElement modelElement)
      {
         // Get the domain class associated with model element.

         DomainClassInfo domainClass = modelElement.GetDomainClass();

         if (domainClass?.AllEmbeddedByDomainRoles?.Count == 1)
         {
            DomainRoleInfo roleInfo = domainClass.AllEmbeddedByDomainRoles[0];

            // Get a collection of all the links to this model element
            // Since this is in a compartment there should be at least one
            // There can be only one.

            ElementLink elementLink = roleInfo.GetElementLinks(modelElement)?.FirstOrDefault();

            return elementLink?.LinkedElements?.FirstOrDefault(e => !modelElement.Equals(e));
         }

         return null;
      }

      /// <summary>
      /// Compares two objects of a specified type and returns a list of tuples containing
      /// the names of properties that are different along with their values from the two objects.
      /// </summary>
      /// <typeparam name="T">The type of object to compare.</typeparam>
      /// <param name="This">The first object to compare.</param>
      /// <param name="Other">The second object to compare.</param>
      /// <returns>A list of tuples containing the names of properties that are different along with their values from the two objects.</returns>
      public static List<(string propertyName, object thisValue, object otherValue)> GetDifferences<T>(this T This, T Other) where T : ModelElement
      {
         List<(string propertyName, object thisValue, object otherValue)> result = new List<(string propertyName, object thisValue, object otherValue)>();
         ReadOnlyCollection<DomainPropertyInfo> domainProperties = This.GetDomainClass().AllDomainProperties;

         foreach (DomainPropertyInfo domainProperty in domainProperties)
         {
            object thisProperty = domainProperty.GetValue(This);
            object otherProperty = domainProperty.GetValue(Other);

            if ((thisProperty == null) && (otherProperty == null))
               continue;

            if ((thisProperty != null) && (otherProperty != null) && thisProperty.Equals(otherProperty))
               continue;

            result.Add((domainProperty.Name, thisProperty, otherProperty));
         }

         return result;
      }

      /// <summary>
      /// Returns the first ShapeElement of a ModelElement.
      /// </summary>
      /// <param name="element">The ModelElement to search.</param>
      /// <returns>The first ShapeElement of the ModelElement.</returns>
      public static ShapeElement GetFirstShapeElement(this ModelElement element)
      {
         return PresentationViewsSubject.GetPresentation(element)
                                        .OfType<ShapeElement>()
                                        .FirstOrDefault(s => s.Diagram == EFModel.ModelRoot.GetCurrentDiagram());
      }

      /// <summary>
      ///    Find the shape element if any, representing the model element on the indicated diagram, or on the current diagram if none is passed
      /// </summary>
      /// <param name="element">ModelElement represented by the shape in question</param>
      /// <param name="diagram">Diagram to interrogate</param>
      /// <returns>ShapeElement if available, null otherwise</returns>
      private static ShapeElement GetShapeElement(this ModelElement element, Diagram diagram = null)
      {
         ShapeElement result = null;

         // Get the shape on the diagram. If not specified, pick the current one
         if (diagram == null)
            diagram = EFModel.ModelRoot.GetCurrentDiagram();

         if (diagram != null)
         {
            result = diagram.Store.GetAll<ShapeElement>().FirstOrDefault(x => (x.ModelElement == element) && (x.Diagram == diagram));

            // If the model element is in a compartment the result should be null? Check for Compartment type just in case
            if (result is Compartment)
            {
               ModelElement parentElement = element.GetCompartmentElementFirstParentElement();
               result = parentElement?.GetShapeElement(diagram);
            }
         }

         return result;
      }

      /// <summary>
      ///    Causes all diagrams to redraw
      /// </summary>
      /// <param name="element"></param>
      public static void InvalidateDiagrams(this ModelElement element)
      {
         if (element != null)
         {
            List<EFModelDiagram> diagrams = element.Store
                                                   .DefaultPartitionForClass(EFModelDiagram.DomainClassId)
                                                   .GetAll<EFModelDiagram>()
                                                   .ToList();

            foreach (EFModelDiagram diagram in diagrams)
               diagram.Invalidate();
         }
      }

      /// <summary>
      ///    Detects if the model element is currently loading into memory from storage
      /// </summary>
      /// <param name="element">Element to interrogate</param>
      /// <returns>True if loading, false otherwise</returns>
      public static bool IsLoading(this ModelElement element)
      {
         TransactionManager transactionManager = element.Store.TransactionManager;

         Transaction currentTransaction = transactionManager.CurrentTransaction;

         return transactionManager.InTransaction
             && (currentTransaction.IsSerializing
              || (currentTransaction.Name.ToLowerInvariant() == "paste")
              || (currentTransaction.Name.ToLowerInvariant() == "local merge transaction"));
      }

      /// <summary>
      ///    Detects whether the model element has a visible shape on the indicated diagram, or on the current diagram if none is passed
      /// </summary>
      /// <param name="modelElement">ModelElement represented by the shape in question</param>
      /// <param name="diagram">Diagram to interrogate</param>
      /// <returns>True if found, false otherwise</returns>
      internal static bool IsVisible(this ModelElement modelElement, Diagram diagram = null)
      {
         return modelElement.GetShapeElement(diagram)?.IsVisible(diagram) == true;
      }

      /// <summary>
      ///    Detects whether the shape element is present and visible on the indicated diagram, or on the current diagram if none is passed
      /// </summary>
      /// <param name="shapeElement">ShapeElement to find</param>
      /// <param name="diagram">Diagram to interrogate</param>
      /// <returns>True if found, false otherwise</returns>
      internal static bool IsVisible(this ShapeElement shapeElement, Diagram diagram = null)
      {
         Diagram targetDiagram = diagram ?? EFModel.ModelRoot.GetCurrentDiagram();

         return (targetDiagram != null) && (shapeElement.Diagram == targetDiagram) && shapeElement.IsVisible;
      }

      /// <summary>
      ///    Gets the root element of the model
      /// </summary>
      /// <param name="store">Store object to search to find the root</param>
      public static ModelRoot ModelRoot(this Store store)
      {
         return store.GetAll<ModelRoot>().FirstOrDefault();
      }

      /// <summary>
      /// Redraws the specified <paramref name="element"/>.
      /// </summary>
      /// <param name="element">The element to redraw.</param>
      public static void Redraw(this ModelElement element)
      {
         // redraw on every diagram
         foreach (ShapeElement shapeElement in PresentationViewsSubject.GetPresentation(element).OfType<ShapeElement>().Distinct().ToList())
            shapeElement.Invalidate();
      }
   }
}