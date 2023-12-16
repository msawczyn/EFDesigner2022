using System;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;

using EnvDTE;

using EnvDTE80;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using VSLangProj;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   /// Provides helper methods for working with menu commands.
   /// </summary>
   [ProvideAutoLoad(EFModelCommandSet.guidEFModelUIContextGuidString, PackageAutoLoadFlags.BackgroundLoad)]
   [ProvideUIContextRule(EFModelCommandSet.guidEFModelUIContextGuidString,
                         "EFModel auto load",
                         "EFModel",
                         new[] {"EFModel"},
                         new[] {"HierSingleSelectionName:.efmodel$"})]
   public static class CommandHelper
   {
      private const string TextTransformationFileExtension = ".tt";

      /// <summary>
      /// Represents the file extension used for EF Modeler files.
      /// </summary>
      public static string EFModelerFileNameExtension = ".efmodel";


      /// <summary>
      /// Generates code based on the provided model file path.
      /// </summary>
      /// <param name="modelFilepath">The file path of the efmodel file.</param>
      public static void GenerateCode(string modelFilepath)
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         EFModelDocData.GenerateCode(modelFilepath);
      }

      /// <summary>
      /// Displays an Open File dialog and returns the selected file path.
      /// </summary>
      public static string GetSingleFileSelectedPath()
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         if (!IsSingleProjectItemSelection(out IVsHierarchy hierarchy, out uint itemid))
            return null;

         // Get the file path
         // ReSharper disable once SuspiciousTypeConversion.Global
         ((IVsProject)hierarchy).GetMkDocument(itemid, out string itemFullPath);

         return itemFullPath;
      }

      /// <summary>
      /// Checks if the specified file has the given extension.
      /// </summary>
      /// <param name="fileInfo">The file to check</param>
      /// <param name="extension">The extension to look for</param>
      /// <returns>True if the file has the extension, otherwise false</returns>
      public static bool HasExtension(this FileInfo fileInfo, string extension)
      {
         return string.Compare(fileInfo.Extension, extension, StringComparison.OrdinalIgnoreCase) == 0;
      }

      private static bool IsSingleProjectItemSelection(out IVsHierarchy hierarchy, out uint itemid)
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         hierarchy = null;
         itemid = VSConstants.VSITEMID_NIL;


         if ((!(Package.GetGlobalService(typeof(SVsShellMonitorSelection)) is IVsMonitorSelection monitorSelection)) 
          || (!(Package.GetGlobalService(typeof(SVsSolution)) is IVsSolution solution)))
            return false;

         IntPtr hierarchyPtr = IntPtr.Zero;
         IntPtr selectionContainerPtr = IntPtr.Zero;

         try
         {
            int hr = monitorSelection.GetCurrentSelection(out hierarchyPtr, out itemid, out IVsMultiItemSelect multiItemSelect, out selectionContainerPtr);

            if (ErrorHandler.Failed(hr) || (hierarchyPtr == IntPtr.Zero) || (itemid == VSConstants.VSITEMID_NIL))
            {
               // there is no selection
               return false;
            }

            // multiple items are selected
            if (multiItemSelect != null)
               return false;

            // there is a hierarchy root node selected, thus it is not a single item inside a project

            if (itemid == VSConstants.VSITEMID_ROOT)
               return false;

            hierarchy = Marshal.GetObjectForIUnknown(hierarchyPtr) as IVsHierarchy;

            if (hierarchy == null)
               return false;

            if (ErrorHandler.Failed(solution.GetGuidOfProject(hierarchy, out Guid _)))
               return false; // hierarchy is not a project inside the Solution if it does not have a ProjectID Guid

            // if we got this far then there is a single project item selected
            return true;
         }
         finally
         {
            if (selectionContainerPtr != IntPtr.Zero)
               Marshal.Release(selectionContainerPtr);

            if (hierarchyPtr != IntPtr.Zero)
               Marshal.Release(hierarchyPtr);
         }
      }

      /// <summary>
      /// Sets the visibility and enabled state of the specified MenuCommand.
      /// </summary>
      /// <param name="menuCommand">The MenuCommand to modify.</param>
      /// <param name="visibleAndEnabled">The visibility and enabled state to set.</param>
      public static void VisibleAndEnabled(this MenuCommand menuCommand, bool visibleAndEnabled)
      {
         menuCommand.Visible = visibleAndEnabled;
         menuCommand.Enabled = visibleAndEnabled;
      }
   }
}