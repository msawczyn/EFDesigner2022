using Microsoft.VisualStudio.Modeling.Diagrams;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   /// Represents a node in a dot graph.
   /// </summary>
   public class DotNode
   {
      /// <summary>
      /// Gets or sets the value of the X coordinate.
      /// </summary>
      public double X { get; set; }
      /// <summary>
      /// Gets or sets the Y coordinate.
      /// </summary>
      public double Y { get; set; }
      /// <summary>
      /// Gets or sets the NodeShape object this graph node represents.
      /// </summary>
      public NodeShape Shape { get; set; }
   }
}