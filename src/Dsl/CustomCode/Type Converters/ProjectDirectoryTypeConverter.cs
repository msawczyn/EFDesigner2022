using System;
using System.Collections.Generic;
using System.ComponentModel;

using EnvDTE;

using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Shell;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   ///    Converts a string path to a ProjectDirectory object and vice versa.
   /// </summary>
   public class ProjectDirectoryTypeConverter : TypeConverterBase
   {
      private List<string> GetProjectDirectories(ProjectItems projectItems, string root)
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         List<string> result = new List<string>();

         if (projectItems != null)
         {
            for (int index = 1; index <= projectItems.Count; index++)
            {
               ProjectItem item = projectItems.Item(index);

               if (item.Kind == Constants.vsProjectItemKindPhysicalFolder)
               {
                  string path = root.Length > 0
                                   ? $"{root}\\{item.Name}"
                                   : item.Name;

                  result.Add(path);
                  result.AddRange(GetProjectDirectories(item.ProjectItems, path));
               }
            }
         }

         return result;
      }

      /// <summary>Returns a collection of standard values for the data type this type converter is designed for when provided with a format context.</summary>
      /// <param name="context">
      ///    An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context that can be used to extract additional information about the environment from which this converter is invoked. This parameter or properties of this parameter can be
      ///    <see langword="null" />.
      /// </param>
      /// <returns>
      ///    A <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection" /> that holds a standard set of valid values, or
      ///    <see langword="null" /> if the data type does not support a standard set of values.
      /// </returns>
      public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         Store store = GetStore(context.Instance);
         DTE dte = store?.GetService(typeof( DTE )) as DTE;
         Array projects = dte?.ActiveSolutionProjects as Array;

         if (projects?.GetValue(0) is Project currentProject)
         {
            List<string> result = new List<string> { string.Empty };
            result.AddRange(GetProjectDirectories(currentProject.ProjectItems, ""));

            return new StandardValuesCollection(result);
         }

         return base.GetStandardValues(context);
      }

      /// <summary>
      ///    Returns whether the collection of standard values returned from
      ///    <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues" /> is an exclusive list of possible values, using the specified context.
      /// </summary>
      /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
      /// <returns>
      ///    <see langword="true" /> if the <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection" />
      ///    returned from <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues" /> is an exhaustive list of possible values;
      ///    <see langword="false" /> if other values are possible.
      /// </returns>
      public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
      {
         return true;
      }

      /// <summary>Returns whether this object supports a standard set of values that can be picked from a list, using the specified context.</summary>
      /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
      /// <returns>
      ///    <see langword="true" /> if <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues" /> should be called to find a common set of values the object supports; otherwise,
      ///    <see langword="false" />.
      /// </returns>
      public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
      {
         return true;
      }
   }
}