using System.Linq;
using Microsoft.VisualStudio.Modeling.Diagrams;
using Microsoft.VisualStudio.Modeling.Diagrams.GraphObject;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   /// Engine that lays out a diagram using the built-in Modeling SDK layout mechanism
   /// </summary>
   public class StandardLayout
   {
      /// <summary>
      ///   Layout the given shape elements in the specified entity framework model diagram, using the standard layout mechanism for the Modeling SDK
      /// </summary>
      /// <param name="diagram">Owning diagram</param>
      public static void Execute(EFModelDiagram diagram)
      {
         // first we need to mark all the connectors as dirty so they'll route. Easiest way is to flip their 'ManuallyRouted' flag
         BinaryLinkShape[] links = diagram.NestedChildShapes.OfType<BinaryLinkShape>().ToArray();

         NodeShape[] nodeShapes = diagram.NestedChildShapes.OfType<NodeShape>()
                                         .Union(links.Select(link => link.FromShape)
                                                     .Union(links.Select(link => link.ToShape))
                                                     .Where(node => node != null))
                                         .Distinct()
                                         .ToArray();

         foreach (BinaryLinkShape linkShape in links)
            linkShape.ManuallyRouted = !linkShape.ManuallyRouted;

         // now let the layout mechanism route the connectors by setting 'ManuallyRouted' to false, regardless of what it was before
         foreach (BinaryLinkShape linkShape in links)
            linkShape.ManuallyRouted = false;

         diagram.AutoLayoutShapeElements(nodeShapes.Cast<ShapeElement>().Union(links).ToList()
                                       , VGRoutingStyle.VGRouteStraight
                                       , PlacementValueStyle.VGPlaceSN
                                       , true);
      }

   }
}
