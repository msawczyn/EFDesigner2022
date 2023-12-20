using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.Modeling.Diagrams;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   ///    Rectangular node in a graph
   /// </summary>
   public class Node
   {
      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="shapeElement">Shape element to represent</param>
      public Node(ShapeElement shapeElement)
      {
         RectangleD boundingBox = shapeElement.AbsoluteBoundingBox;

         ElementId = shapeElement.ModelElement.Id;
         Left = boundingBox.Left;
         Top = boundingBox.Top;
         Width = boundingBox.Width;
         Height = boundingBox.Height;
      }

      /// <summary>
      ///   Process the given diagram and return a list of nodes
      /// </summary>
      /// <param name="diagram">Source diagram</param>
      public static List<Node> ProcessDiagram(EFModelDiagram diagram)
      {
         List<Node> nodes = new List<Node>();

         foreach (NodeShape shapeElement in diagram.NestedChildShapes.OfType<NodeShape>())
            nodes.Add(new Node(shapeElement));

         foreach (Node fromNode in nodes)
         {
            List<BinaryLinkShape> links = diagram.NestedChildShapes
                                                 .OfType<BinaryLinkShape>()
                                                 .Where(link => link.FromShape.ModelElement.Id == fromNode.ElementId)
                                                 .Distinct()
                                                 .ToList();

            foreach (BinaryLinkShape linkShape in links)
               fromNode.Edges.Add(new Edge { Destination = nodes.Single(n => n.ElementId == linkShape.ToShape.ModelElement.Id) });
         }

         return nodes;
      }

      /// <summary>
      /// Id of the element represented by this node
      /// </summary>
      public Guid ElementId { get; set; }

      /// <summary>
      ///    Connectors between this node and other nodes
      /// </summary>
      public List<Edge> Edges { get; } = new List<Edge>();

      /// <summary>
      ///    Force applied to this node
      /// </summary>
      public Vector Force { get; set; } = new Vector(0, 0);

      /// <summary>
      ///    Height of this node
      /// </summary>
      public double Height { get; set; }

      /// <summary>
      ///    Left position of this node
      /// </summary>
      public double Left { get; set; }

      /// <summary>
      ///    Top position of this node
      /// </summary>
      public double Top { get; set; }

      /// <summary>
      /// X coordinate of the right side of this node
      /// </summary>
      public double Right => Left + Width;

      /// <summary>
      /// Y coordinate of the bottom of this node
      /// </summary>
      public double Bottom => Top + Height;

      /// <summary>
      ///    Velocity of this node (attract/repel)
      /// </summary>
      public Vector Velocity { get; set; } = new Vector(0, 0);

      /// <summary>
      ///    Width of this node
      /// </summary>
      public double Width { get; set; }
   }
}