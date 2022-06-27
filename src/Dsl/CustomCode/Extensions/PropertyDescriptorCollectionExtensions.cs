using System.ComponentModel;
using System.Linq;

namespace Sawczyn.EFDesigner.EFModel
{
   public static class PropertyDescriptorCollectionExtensions
   {
      public static void Remove(this PropertyDescriptorCollection propertyDescriptors, string name)
      {
         PropertyDescriptor propertyDescriptor = propertyDescriptors.OfType<PropertyDescriptor>().SingleOrDefault(x => x.Name == name);

         if (propertyDescriptor != null)
            propertyDescriptors.Remove(propertyDescriptor);
      }
   }
}