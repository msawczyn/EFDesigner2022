#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

using EnvDTE;

namespace Sawczyn.EFDesigner.EFModel.EditingOnly
{
   [SuppressMessage("ReSharper", "UnusedMember.Local")]
   [SuppressMessage("ReSharper", "UnusedMember.Global")]
   public partial class GeneratedTextTransformation
   {
      #region Template

      // EFDesigner v4.2.7
      // Copyright (c) 2017-2024 Michael Sawczyn
      // https://github.com/msawczyn/EFDesigner

      // this bit is based on EntityFramework Reverse POCO Code First Generator
      // Copyright (C) Simon Hughes 2012
      // https://github.com/sjh37/EntityFramework-Reverse-POCO-Code-First-Generator
      //

      /**************************************************
       * Interactions with Visual Studio
       */

      /// <summary>
      /// Retrieves all projects from the current solution.
      /// </summary>
      public IEnumerable<Project> GetAllProjects()
      {
         foreach (Project project in GetSolution().Projects.OfType<Project>())
         {
            if (project.Kind == EnvDTE.Constants.vsProjectKindSolutionItems)
            {
               foreach (Project p in RecurseSolutionFolder(project))
                  yield return p;
            }
            else
               yield return project;
         }
      }

      /// <summary>
      /// Gets the currently active project.
      /// </summary>
      /// <returns>The current project.</returns>
      public Project GetCurrentProject()
      {
         DTE dte = GetDTE();

         ProjectItem projectItem = dte.Solution.FindProjectItem(Host.TemplateFile);

         if (projectItem?.ContainingProject != null)
            return projectItem.ContainingProject;

         // this returns SELECTED (active) project(s) - it may be a different project than the T4 template. 
         Array activeSolutionProjects = (Array)dte.ActiveSolutionProjects ?? throw new Exception("DTE.ActiveSolutionProjects returned null");
         if (activeSolutionProjects.Length > 0)
         {
            Project dteProject = (Project)activeSolutionProjects.GetValue(0);

            if (dteProject != null)
               return dteProject;
         }

         throw new InvalidOperationException("Error in GetCurrentProject(). Unable to find project.");
      }

      //private ProjectItem GetDirectoryItem(string target)
      //{
      //   DTE dte = GetDTE();
      //   Array projects = dte?.ActiveSolutionProjects as Array;
      //   ProjectItem targetProjectItem = null;

      //   if (projects?.GetValue(0) is Project currentProject)
      //   {
      //      string rootDirectory = Path.GetDirectoryName(currentProject.FullName);
      //      Directory.CreateDirectory(Path.Combine(rootDirectory, target));

      //      Queue<string> paths = new Queue<string>(target.Split('\\'));
      //      ProjectItems currentItemList = currentProject.ProjectItems;
      //      bool found = false;

      //      while (paths.Any())
      //      {
      //         string path = paths.Dequeue();

      //         for (int index = 1; index <= currentItemList.Count; ++index)
      //         {
      //            if (currentItemList.Item(index).Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder)
      //            {
      //               if (!paths.Any())
      //                  targetProjectItem = currentItemList.Item(index);
      //               else
      //                  currentItemList = currentItemList.Item(index).ProjectItems;

      //               found = true;

      //               break;
      //            }
      //         }

      //         if (!found)
      //         {
      //            ProjectItem newItem = currentItemList.AddFolder(path);

      //            if (!paths.Any())
      //               targetProjectItem = newItem;
      //            else
      //               currentItemList = newItem.ProjectItems;
      //         }
      //      }
      //   }

      //   return targetProjectItem;
      //}

      /// <summary>
      /// Gets the instance of the Visual Studio development environment.
      /// </summary>
      /// <returns>The instance of the development environment.</returns>
      public DTE GetDTE()
      {
         IServiceProvider serviceProvider = (IServiceProvider)Host ?? throw new Exception("Host property returned unexpected value (null)");
         DTE dte = (DTE)serviceProvider.GetService(typeof(DTE)) ?? throw new Exception("Unable to retrieve EnvDTE.DTE");
         return dte;
      }

      //private string GetProjectPath(Project project)
      //{
      //   string fullProjectName = project.FullName;

      //   if (string.IsNullOrWhiteSpace(fullProjectName))
      //      return string.Empty;

      //   try
      //   {
      //      FileInfo info = new FileInfo(fullProjectName);

      //      return info.Directory != null
      //                ? info.Directory.FullName
      //                : string.Empty;
      //   }
      //   catch
      //   {
      //      WriteLine("// Project " + fullProjectName + " excluded.");

      //      return string.Empty;
      //   }
      //}

      /// <summary>
      /// This method returns a Solution object
      /// </summary>
      public Solution GetSolution()
      {
         return GetDTE().Solution;
      }

      private IEnumerable<Project> RecurseSolutionFolder(Project project)
      {
         if (project.ProjectItems == null)
            yield break;

         foreach (Project subProject in project.ProjectItems
                                               .Cast<ProjectItem>()
                                               .Select(projectItem => projectItem.SubProject)
                                               .Where(subProject => subProject != null))
         {
            if (subProject.Kind == EnvDTE.Constants.vsProjectKindSolutionItems)
            {
               foreach (Project p in RecurseSolutionFolder(subProject))
                  yield return p;
            }
            else
               yield return subProject;
         }
      }

#endregion Template
   }
}