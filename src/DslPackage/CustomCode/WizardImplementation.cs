using System.Collections.Generic;

using EnvDTE;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TemplateWizard;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   /// A class that implements the IWizard interface and provides the logic for a wizard.
   /// </summary>
   public class WizardImplementation : IWizard
   {
      private static string modelPath;
      private static string diagramPath;
      private static string xsdPath;
      private static DTE dte;

      /// <summary>
      /// This method is called when the wizard starts running.
      /// </summary>
      /// <param name="automationObject">The automation object provided by the Visual Studio IDE.</param>
      /// <param name="replacementsDictionary">The dictionary of replacement parameter names and their values.</param>
      /// <param name="runKind">The kind of wizard run.</param>
      /// <param name="customParams">The array of custom parameters.</param>
      public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams) { }

      /// <summary>
      /// Method called when project generation is finished.
      /// </summary>
      /// <param name="project">The generated project.</param>
      public void ProjectFinishedGenerating(Project project) { }

      /// <summary>
      /// Event handler for when a project item has finished being generated.
      /// </summary>
      /// <param name="projectItem">The generated project item.</param>
      public void ProjectItemFinishedGenerating(ProjectItem projectItem)
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         dte = dte ?? projectItem.DTE;
         string path = projectItem.FileNames[0];

         if (path.EndsWith(".efmodel"))
            modelPath = path;
         else if (path.EndsWith(".diagram"))
            diagramPath = path;
         else if (path.EndsWith(".xsd"))
            xsdPath = path;
      }

      /// <summary>
      /// Determines whether a project item should be added based on the provided file path.
      /// </summary>
      /// <param name="filePath">The file path to check.</param>
      /// <returns>Returns true if the project item should be added, false otherwise.</returns>
      public bool ShouldAddProjectItem(string filePath)
      {
         return true;
      }

      /// <summary>
      /// Method executed before opening a file in the project.
      /// </summary>
      /// <param name="projectItem">Item to be opened.</param>
      public void BeforeOpeningFile(ProjectItem projectItem) { }

      /// <summary>
      /// Signals that the test run has finished.
      /// </summary>
      public void RunFinished()
      {
         // The VSIX can't nest files, so we'll do that here
         // NOTE: Don't nest the .tt file -- it doesn't seem to like that, and bad things happen

         ThreadHelper.ThrowIfNotOnUIThread();

         if (modelPath != null && dte != null)
         {
            ProjectItem modelItem = dte.Solution.FindProjectItem(modelPath);

            if (modelItem != null)
            {
               if (diagramPath != null)
               {
                  ProjectItem diagramItem = dte.Solution.FindProjectItem(diagramPath);

                  if (diagramItem != null)
                  {
                     diagramItem.Remove();
                     modelItem.ProjectItems.AddFromFile(diagramPath);
                  }
               }

               if (xsdPath != null)
               {
                  ProjectItem xsdItem = dte.Solution.FindProjectItem(xsdPath);

                  if (xsdItem != null)
                  {
                     xsdItem.Remove();
                     modelItem.ProjectItems.AddFromFile(xsdPath);
                  }
               }
            }
         }

         diagramPath = null;
         modelPath = null;
         xsdPath = null;
         dte = null;
      }
   }
}