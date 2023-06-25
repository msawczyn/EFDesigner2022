using System.ComponentModel;
using System.Linq;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   ///    Extension methods for PropertyDescriptorCollection
   /// </summary>
   public static class PropertyDescriptorCollectionExtensions
   {
      /// <summary>
      /// Removes a property descriptor with the given name from the collection.
      /// </summary>
      /// <param name="propertyDescriptors">The collection to remove the property descriptor from.</param>
      /// <param name="name">The name of the property descriptor to remove.</param>
      public static void Remove(this PropertyDescriptorCollection propertyDescriptors, string name)
      {
         PropertyDescriptor propertyDescriptor = propertyDescriptors.OfType<PropertyDescriptor>().SingleOrDefault(x => x.Name == name);

         if (propertyDescriptor != null)
            propertyDescriptors.Remove(propertyDescriptor);
      }
   }
}