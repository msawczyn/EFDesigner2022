using System;

using EnvDTE;

using Microsoft.VisualStudio.Shell;

namespace Sawczyn.EFDesigner.EFModel
{
   public static class ProjectExtensions
   {
      public static string TargetFrameworkVersion(this Project project)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         if (project == null)
            throw new ArgumentNullException(nameof( project ));

         return project.Properties.Item("TargetFramework")?.Value?.ToString();
      }
   }
}