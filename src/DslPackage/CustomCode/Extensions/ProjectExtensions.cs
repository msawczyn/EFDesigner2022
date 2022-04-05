using System;

using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace Sawczyn.EFDesigner.EFModel
{
   public static class ProjectExtensions
   {
      public static string TargetFrameworkVersion(this Project project)
      {
         if (project == null)
            throw new ArgumentNullException(nameof(project));

         ThreadHelper.ThrowIfNotOnUIThread();
         return project.Properties.Item("TargetFramework")?.Value?.ToString();
      }
   }
}
