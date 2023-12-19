using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Diagrams;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   ///    Force-directed graph layout algorithm
   /// </summary>
   public class FDGraphLayout
   {
      private static void AdjustNodeWidth(Node currentNode, Node[] allNodes)
      {
         // ReSharper disable once LoopCanBePartlyConvertedToQuery
         // Logic for modifying widths 
         foreach (Node otherNode in allNodes)
         {
            if (currentNode != otherNode && OverlapX(currentNode, otherNode))
            {
               // Check for overlap in the X-axis
               // Calculate the minimum required gap to avoid overlap
               double requiredGap = otherNode.Left - (currentNode.Left + currentNode.Width);

               // Adjust the width if overlap is detected
               if (requiredGap > 0)
                  currentNode.Width += requiredGap;
            }
         }
      }

      private static void ApplyAttractionForce(Node node1, Node node2, double springConstant)
      {
         Vector direction = new Vector(node2.Left - node1.Left, node2.Top - node1.Top);
         double distance = direction.Length();

         if (distance > 0)
         {
            double forceMagnitude = springConstant * distance;
            node1.Force += direction * forceMagnitude;
            node2.Force -= direction * forceMagnitude;
         }
      }

      private static void ApplyRepulsionForce(Node node1, Node node2, double repulsionFactor)
      {
         Vector direction = new Vector(node2.Left - node1.Left, node2.Top - node1.Top);
         double distanceSquared = direction.LengthSquared();

         if (distanceSquared > 0)
         {
            double forceMagnitude = repulsionFactor / distanceSquared;
            node1.Force -= direction * forceMagnitude;
            node2.Force += direction * forceMagnitude;
         }
      }

      /// <summary>
      /// Layout the given shape elements in the specified entity framework model diagram.
      /// </summary>
      /// <param name="diagram">Owning diagram</param>
      /// <param name="connectors">Connections between nodes (graph edges)</param>
      public static void Execute(EFModelDiagram diagram, IEnumerable<BinaryLinkShape> connectors)
      {
         using (Transaction transaction = diagram.Store.TransactionManager.BeginTransaction("Layout Diagram"))
         {
            LayoutInternal(diagram, connectors);
            transaction.Commit();
         }
      }

      private static void LayoutInternal(EFModelDiagram diagram, IEnumerable<BinaryLinkShape> connectors)
      {
         BinaryLinkShape[] links = connectors.ToArray();

         NodeShape[] nodeShapes = links.Select(link => link.FromShape)
                                       .Union(links.Select(link => link.ToShape))
                                       .Where(node => node != null)
                                       .Distinct()
                                       .ToArray();

         Node[] nodes = Node.ProcessDiagram(nodeShapes).ToArray();

         const int iterations = 100; // Adjust as needed
         const double springConstant = 0.1f; // Tweak for optimal results
         const double repulsionFactor = 100f; // Tweak for optimal results
         const double dampingFactor = 0.8f; // Tweak for optimal results

         //Random random = new Random();

         // Initialize positions and velocities
         foreach (Node node in nodes)
         {
            //node.Left = random.Next();
            //node.Top = random.Next();
            node.Velocity = new Vector(0, 0);
            node.Force = new Vector(0, 0);
         }

         for (int iteration = 0; iteration < iterations; iteration++)
         {
            // Calculate forces
            foreach (Node node in nodes)
            {
               foreach (Node otherNode in nodes.Where(otherNode => node != otherNode))
                  ApplyRepulsionForce(node, otherNode, repulsionFactor);

               foreach (Edge edge in node.Edges)
                  ApplyAttractionForce(node, edge.Destination, springConstant);
            }

            // Integrate forces to update positions
            foreach (Node node in nodes)
            {
               // Use Euler method for integration
               node.Velocity += node.Force;
               node.Left += node.Velocity.X;
               node.Top += node.Velocity.Y;

               // Apply damping to simulate friction
               node.Velocity *= dampingFactor;

               // Reset forces for the next iteration
               node.Force = new Vector(0, 0);
            }
         }

         // Update node widths and edge segments
         foreach (Node node in nodes)
         {
            // Adjust node widths to avoid overlaps
            AdjustNodeWidth(node, nodes);

            // Update edge segments to connect nodes
            foreach (Edge edge in node.Edges)
            {
               edge.Segments.Clear();
               UpdateEdgeSegments(edge);
            }
         }

         Node leftmost = nodes.OrderBy(n => n.Left).First();
         Node topmost = nodes.OrderBy(n => n.Top).First();

         foreach (Node node in nodes)
         {
            node.Left -= leftmost.Left;
            node.Top -= topmost.Top;
         }

         double width = nodes.OrderByDescending(n => n.Right).First().Right;
         double height = nodes.OrderByDescending(n => n.Bottom).First().Bottom;

         diagram.AbsoluteBounds = new RectangleD(diagram.AbsoluteBounds.X
                                               , diagram.AbsoluteBounds.Y
                                               , Math.Max(diagram.AbsoluteBounds.Width, width)
                                               , Math.Max(diagram.AbsoluteBounds.Height, height));

         foreach (Node node in nodes)
         {
            NodeShape shapeElement = diagram.NestedChildShapes.OfType<NodeShape>().FirstOrDefault(s => s.ModelElement.Id == node.ElementId);

            if (shapeElement != null)
               shapeElement.Bounds = new RectangleD(node.Left, node.Top, node.Width, node.Height);
         }

         diagram.Invalidate();
      }

      private static bool OverlapX(Node node1, Node node2)
      {
         // Check if there is an overlap in the X-axis
         return node1.Left < node2.Left + node2.Width && node1.Left + node1.Width > node2.Left;
      }

      private static void UpdateEdgeSegments(Edge edge)
      {
         // Logic to calculate and add line segments goes here
         // Ensure edges connect nodes without crossing other rectangles
         // For simplicity, let's connect nodes directly without considering obstacles
         foreach (Edge nodeSegment in edge.Destination.Edges)
         {
            edge.Segments.Add(new LineSegment
            {
               BeginX = edge.Destination.Left
                               ,
               BeginY = edge.Destination.Top
                               ,
               EndX = nodeSegment.Destination.Left
                               ,
               EndY = nodeSegment.Destination.Top
            });
         }
      }
   }
}