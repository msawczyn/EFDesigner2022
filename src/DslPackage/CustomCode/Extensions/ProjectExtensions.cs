using System;

using EnvDTE;

using Microsoft.VisualStudio.Shell;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   /// Provides extension methods for Project class.
   /// </summary>
   public static class ProjectExtensions
   {
      /// <summary>
      /// Gets the target framework version of the specified project.
      /// </summary>
      /// <param name="project">The project to get the target framework version for.</param>
      /// <returns>The target framework version.</returns>
      public static string TargetFrameworkVersion(this Project project)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         if (project == null)
            throw new ArgumentNullException(nameof( project ));

         return project.Properties.Item("TargetFramework")?.Value?.ToString();
      }
   }
}