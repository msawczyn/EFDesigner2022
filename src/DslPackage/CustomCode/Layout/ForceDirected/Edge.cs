using System.Collections.Generic;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   ///    Connectors between nodes
   /// </summary>
   public class Edge
   {
      /// <summary>
      ///    Destination node, relative to the owner of this edge
      /// </summary>
      public Node Destination { get; set; }

      /// <summary>
      ///    Line segments making up this edge
      /// </summary>
      public List<LineSegment> Segments { get; } = new List<LineSegment>();
   }
}