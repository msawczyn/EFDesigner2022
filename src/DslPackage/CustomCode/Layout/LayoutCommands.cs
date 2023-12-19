using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Diagrams;

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
            IEnumerable<ShapeElement> shapeElements = diagram.NestedChildShapes.Where(s => s.IsVisible);

            LayoutDiagram(diagram, shapeElements);
         }
      }

      /// <summary>
      /// Arranges the position of the given shape elements in the specified entity framework model diagram.
      /// </summary>
      /// <param name="diagram">The entity framework model diagram to layout.</param>
      /// <param name="shapeElements">The shape elements to arrange in the layout.</param>
      public static void LayoutDiagram(EFModelDiagram diagram, IEnumerable<ShapeElement> shapeElements)
      {
         using (Transaction tx = diagram.Store.TransactionManager.BeginTransaction("ModelAutoLayout"))
         {
            IEnumerable<BinaryLinkShape> connectors = shapeElements.OfType<BinaryLinkShape>().Where(link => link.FromShape != null && link.ToShape != null);

            // use graphviz as the default if available
            if (File.Exists(EFModelPackage.Options.DotExePath))
               GraphVizLayout.Execute(diagram, connectors);
            else
               //StandardLayout.Execute(diagram, binaryLinkShapes);
               FDGraphLayout.Execute(diagram, connectors);

            tx.Commit();
         }
      }
   }
}