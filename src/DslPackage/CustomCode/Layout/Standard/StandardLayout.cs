using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Modeling.Diagrams;
using Microsoft.VisualStudio.Modeling.Diagrams.GraphObject;

namespace Sawczyn.EFDesigner.EFModel
{
   public class StandardLayout
   {
      public static void Execute(EFModelDiagram diagram, IEnumerable<ShapeElement> shapeElements)
      {
         // first we need to mark all the connectors as dirty so they'll route. Easiest way is to flip their 'ManuallyRouted' flag
         List<BinaryLinkShape> binaryLinkShapes = shapeElements.OfType<BinaryLinkShape>()
                                                               .Where(link => link.FromShape != null && link.ToShape != null)
                                                               .ToList();

         foreach (BinaryLinkShape linkShape in binaryLinkShapes)
            linkShape.ManuallyRouted = !linkShape.ManuallyRouted;

         // now let the layout mechanism route the connectors by setting 'ManuallyRouted' to false, regardless of what it was before
         foreach (BinaryLinkShape linkShape in binaryLinkShapes)
            linkShape.ManuallyRouted = false;

         diagram.AutoLayoutShapeElements(diagram.NestedChildShapes.Where(s => s.IsVisible).ToList(), VGRoutingStyle.VGRouteStraight, PlacementValueStyle.VGPlaceSN, true);
      }

   }
}
