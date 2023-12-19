using Microsoft.VisualStudio.Modeling.Diagrams;

using QuikGraph;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   /// Represents an edge in a DOT graph.
   /// </summary>
   public class DotEdge : IEdge<DotNode>
   {
      /// <summary>
      /// Initializes a new instance of the DotEdge class with the specified source and target nodes.
      /// </summary>
      /// <param name="source">The source node of the edge.</param>
      /// <param name="target">The target node of the edge.</param>
      public DotEdge(DotNode source, DotNode target)
      {
         Source = source;
         Target = target;
      }

      /// <summary>
      /// Gets or sets the BinaryLinkShape object this graph edge represents.
      /// </summary>
      public BinaryLinkShape Shape { get; set; }
      /// <summary>
      /// Gets or sets the source of the dot node.
      /// </summary>
      public DotNode Source { get; set; }
      /// <summary>
      /// Gets or sets the target DotNode.
      /// </summary>
      public DotNode Target { get; set; }
   }
}