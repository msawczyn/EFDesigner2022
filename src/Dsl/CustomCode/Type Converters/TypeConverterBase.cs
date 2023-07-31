using System.ComponentModel;
using System.Linq;

using Microsoft.VisualStudio.Modeling;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   /// Provides a base class for implementing type converters.
   /// </summary>
   public class TypeConverterBase : TypeConverter
   {
      /// <summary>
      /// Retrieves the selected elements from a grid selection.
      /// </summary>
      /// <param name="gridSelection">The grid selection object containing selected elements.</param>
      /// <returns>An array of elements implementing the IHasStore interface.</returns>
      protected IHasStore[] GetSelectedElements(object gridSelection)
      {
         object[] objects = gridSelection as object[];

         return objects?.Cast<IHasStore>().ToArray() ?? new[] {(IHasStore)gridSelection};
      }

      /// <summary>
      ///    Attempts to get to a store from the currently selected object(s) in the property grid.
      /// </summary>
      protected Store GetStore(object gridSelection)
      {
         // We assume that "instance" will either be a single model element, or   
         // an array of model elements (if multiple items are selected).  

         IHasStore currentElement = gridSelection is object[] objects && (objects.Length > 0)
                                       ? objects[0] as IHasStore
                                       : gridSelection as IHasStore;

         return currentElement?.Store;
      }
   }
}