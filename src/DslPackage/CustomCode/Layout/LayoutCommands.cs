using Microsoft.VisualStudio.Modeling;
using System.IO;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   ///    Represents a collection of commands.
   /// </summary>
   public class Commands
   {
      /// <summary>
      /// Arranges the layout of the given EFModelDiagram.
      /// </summary>
      /// <param name="diagram">The EFModelDiagram to layout.</param>
      public static void LayoutDiagram(EFModelDiagram diagram)
      {
         using (WaitCursor _ = new WaitCursor())
         {
            using (Transaction tx = diagram.Store.TransactionManager.BeginTransaction("ModelAutoLayout"))
            {
               // use graphviz as the default if available
               if (File.Exists(EFModelPackage.Options.DotExePath))
                  GraphVizLayout.Execute(diagram);
               else
                  //StandardLayout.Execute(diagram);
                  FDGraphLayout.Execute(diagram);

               tx.Commit();
            }
         }
      }
   }
}