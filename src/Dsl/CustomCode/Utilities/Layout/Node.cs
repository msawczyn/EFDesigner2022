using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Modeling.Diagrams;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   ///    Rectangular node in a graph
   /// </summary>
   public class Node
   {
      public static List<Node> ProcessDiagram(IEnumerable<NodeShape> shapeElements)
      {
         List<Node> result = new List<Node>();

         foreach (NodeShape shapeElement in shapeElements)
         {
            Node node = new Node
            {
               ElementId = shapeElement.ModelElement.Id,
               Left = shapeElement.AbsoluteBoundingBox.Left,
               Top = shapeElement.AbsoluteBoundingBox.Top,
               Width = shapeElement.AbsoluteBoundingBox.Width,
               Height = shapeElement.AbsoluteBoundingBox.Height
            };

            result.Add(node);
         }

         return result;
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
      public Vector Force { get; set; }

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
      public Vector Velocity { get; set; }

      /// <summary>
      ///    Width of this node
      /// </summary>
      public double Width { get; set; }
   }
}