using System.Reflection;

namespace GPTOutput
{
   public class Node : IDisposable
   {
      // list of references to connected nodes (children)
      private readonly List<Node> _connections;

      public Node()
      {
         Location = Point.Empty;
         _connections = new List<Node>();
      }

      /// <summary>
      ///    List of references to connected nodes (children)
      /// </summary>
      public IList<Node> Connections
      {
         get { return _connections.AsReadOnly(); }
      }

      public Point Force { get; set; }

      public double ForceX
      {
         get { return Force.X; }
         set { Force = new Point( (int) value, Force.Y ); }
      }

      public double ForceY
      {
         get { return Force.Y; }
         set { Force = new Point( Force.X, (int) value ); }
      }

      /// <summary>
      ///    Gets or sets the position of the node.
      /// </summary>
      public Point Location { get; set; }

      /// <summary>
      ///    Gets the size of the node (for drawing purposes).
      /// </summary>
      public Size Size { get; }

      /// <summary>
      ///    Gets the X coordinate of the node, relative to the origin.
      /// </summary>
      public int X
      {
         get { return Location.X; }
      }

      /// <summary>
      ///    Gets the Y coordinate of the node, relative to the origin.
      /// </summary>
      public int Y
      {
         get { return Location.Y; }
      }

      public int Left
      {
         get { return X; }
      }

      public int Top
      {
         get { return Y; }
      }

      public int Right
      {
         get { return X + Size.Width; }
      }

      public int Bottom
      {
         get { return Y + Size.Height; }
      }

      public bool Intersects( Node other )
      {
         return Left < other.Right
             && Right > other.Left
             && Top < other.Bottom
             && Bottom > other.Top;
      }

      public bool Intersects( Edge edge )
      {
         return edge.Segments.Any( Intersects );
      }

      public bool Intersects( LineSegment segment )
      {
         int segmentRight = Math.Max( segment.StartX, segment.EndX );
         int segmentLeft = Math.Min( segment.StartX, segment.EndX );
         int segmentTop = Math.Min( segment.StartY, segment.EndY );
         int segmentBottom = Math.Max( segment.StartY, segment.EndY );

         if (Left > segmentRight || Right < segmentLeft)
         {
            return false;
         }

         if (Top < segmentTop || Bottom > segmentBottom)
         {
            return false;
         }


         float yAtRectLeft = segment.CalculateYForX( X );
         float yAtRectRight = segment.CalculateYForX( X + Size.Width );

         if (Bottom > yAtRectLeft && Bottom > yAtRectRight)
         {
            return false;
         }

         if (Top < yAtRectLeft && Top < yAtRectRight)
         {
            return false;
         }

         return true;
      }

      public void Dispose()
      {
         GC.SuppressFinalize( this );
      }

      /// <summary>
      ///    Connects the specified child node to this node.
      /// </summary>
      /// <param name="child">The child node to add.</param>
      /// <returns>True if the node was connected to this node.</returns>
      public bool AddChild( Node child )
      {
         if (child == null)
            throw new ArgumentNullException( nameof( child ) );

         if (child != this && !_connections.Contains( child ))
         {
            _connections.Add( child );
            return true;
         }

         return false;
      }

      /// <summary>
      ///    Connects this node to the specified parent node.
      /// </summary>
      /// <param name="parent">The node to connect to this node.</param>
      /// <returns>True if the other node was connected to this node.</returns>
      public bool AddParent( Node parent )
      {
         if (parent == null)
            throw new ArgumentNullException( nameof( parent ) );
         return parent.AddChild( this );
      }

      /// <summary>
      ///    Removes any connection between this node and the specified node.
      /// </summary>
      /// <param name="other">The other node whose connection is to be removed.</param>
      /// <returns>True if a connection existed.</returns>
      public bool Disconnect( Node other )
      {
         bool c = _connections.Remove( other );
         bool p = other._connections.Remove( this );
         return c || p;
      }
   }

   public class Edge
   {
      public Node Source { get; set; }
      public Node Destination { get; set; }
      public List<LineSegment> Segments { get; } = new List<LineSegment>();

      public Point Start
      {
         get
         {
            return Segments.Count > 0
                      ? Segments[0].Start
                      : Point.Empty;
         }
      }

      public Point End
      {
         get
         {
            return Segments.Count > 0
                      ? Segments[^1].End
                      : Point.Empty;
         }
      }

      public Edge( Node source, Node destination )
      {
         Source = source;
         Destination = destination;
      }
   }

   public class LineSegment
   {
      public Point Start { get; set; }
      public Point End { get; set; }

      public int StartX
      {
         get { return Start.X; }
         set { Start = new Point( value, Start.Y ); }
      }

      public int StartY
      {
         get { return Start.Y; }
         set { Start = new Point( Start.X, value ); }
      }

      public int EndX
      {
         get { return End.X; }
         set { End = new Point( value, End.Y ); }
      }

      public int EndY
      {
         get { return End.Y; }
         set { End = new Point( End.X, value ); }
      }

      public int Left
      {
         get { return Math.Min( StartX, EndX ); }
      }

      public int Top
      {
         get { return Math.Min( StartY, EndY ); }
      }

      public int Right
      {
         get { return Math.Max( StartX, EndX ); }
      }

      public int Bottom
      {
         get { return Math.Max( StartY, EndY ); }
      }

      public int CalculateYForX( int x )
      {
         float m = (float) ( End.Y - Start.Y ) / ( End.X - Start.X );
         float b = Start.Y - m * Start.X;
         return (int) ( m * x + b );
      }
   }


   public static class Graph
   {
      private const float CoulombConstant = 5000.0f;
      private const double DampingFactor = 0.9f;
      private const float HookeConstant = 1.0f;
      private const int MaxIterations = 100;
      private const double optimalDistance = 20;

      private const double SpringLength = 100;
      private const double timeStep = 2;
      private static double temperature = 100;

      private static void ApplyAttractiveForce( Node nodeA, Node nodeB )
      {
         // Calculate the distance between the two nodes
         double distance = CalculateDistance( nodeA.Location, nodeB.Location );

         // Calculate the force between the two nodes using Coulomb's law
         double force = HookeConstant * Math.Pow( distance, 2 ) / SpringLength;

         // Calculate the direction of the force
         double direction = CalculateDirection( nodeA.Location, nodeB.Location );

         // Apply the force to both nodes
         nodeA.Location = new Point( nodeA.Location.X + (int) ( force * Math.Cos( direction ) ), nodeA.Location.Y + (int) ( force * Math.Sin( direction ) ) );
         nodeB.Location = new Point( nodeB.Location.X - (int) ( force * Math.Cos( direction ) ), nodeB.Location.Y - (int) ( force * Math.Sin( direction ) ) );
      }

      private static void ApplyRepulsiveForce( Node nodeA, Node nodeB )
      {
         // Calculate the distance between the two nodes.
         double distance = CalculateDistance( nodeA.Location, nodeB.Location );

         // Calculate the force between the two nodes using Coulomb's law.
         double force = CoulombConstant * CoulombConstant / distance;

         // Calculate the direction of the force.
         double direction = CalculateDirection( nodeA.Location, nodeB.Location );

         // Apply the force to both nodes.
         nodeA.Force = new Point( nodeA.Force.X + (int) ( force * Math.Cos( direction ) ), nodeA.Force.Y + (int) ( force * Math.Sin( direction ) ) );
         nodeB.Force = new Point( nodeB.Force.X - (int) ( force * Math.Cos( direction ) ), nodeB.Force.Y - (int) ( force * Math.Sin( direction ) ) );
      }

      private static void ApplyForces( IEnumerable<Node> nodes )
      {
         // Calculate the repulsive force between each pair of nodes.
         foreach ( Node node1 in nodes )
         {
            foreach ( Node node2 in nodes )
            {
               if (node1 != node2)
                  ApplyRepulsiveForce( node1, node2 );
            }
         }

         // Calculate the attractive force between each pair of connected nodes.
         foreach ( Node node in nodes )
         {
            foreach ( Node neighbor in node.Connections )
               ApplyAttractiveForce( node, neighbor );
         }

         // Update the positions of the nodes.
         foreach ( Node node in nodes )
         {
            node.Location = new Point( node.Location.X + node.Force.X, node.Location.Y + node.Force.Y );
            node.Force = Point.Empty;
         }
      }

      private static void AssignInitialNodePositions( IEnumerable<Node> nodes, Size surfaceSize )
      {
         int nodeCount = nodes.Count();
         int rows = (int) Math.Sqrt( nodeCount );
         int cols = (int) Math.Ceiling( (double) nodeCount / rows );

         int cellWidth = surfaceSize.Width / cols;
         int cellHeight = surfaceSize.Height / rows;

         int x = cellWidth / 2;
         int y = cellHeight / 2;

         foreach ( Node node in nodes )
         {
            node.Location = new Point( x, y );

            x += cellWidth;

            if (x >= surfaceSize.Width)
            {
               x = cellWidth / 2;
               y += cellHeight;
            }
         }
      }

      private static double CalculateDirection( Point nodeALocation, Point nodeBLocation )
      {
         // Calculate the direction of the force between two nodes
         double dx = nodeALocation.X - nodeBLocation.X;
         double dy = nodeALocation.Y - nodeBLocation.Y;
         return Math.Atan2( dy, dx );
      }

      private static double CalculateDistance( Point point1, Point point2 )
      {
         // Calculate the distance between two points.
         double dx = point1.X - point2.X;
         double dy = point1.Y - point2.Y;
         return Math.Sqrt( dx * dx + dy * dy );
      }

      //private static void CalculateEdgeSegments(IEnumerable<Edge> edges)
      //{
      //   // Calculate the edge segments for each edge in the graph.
      //   foreach (Edge edge in edges)
      //   {
      //      // Get the source and destination nodes.
      //      Node source = edge.Source;
      //      Node dest = edge.Destination;

      //      // Calculate the minimum number of segments needed to connect the nodes.
      //      int numSegments = CalculateNumSegments(source, dest);

      //      // Calculate the segment points.
      //      List<Point> segmentPoints = CalculateSegmentPoints(source, dest, numSegments);

      //      // Create the line segments for the edge.
      //      for (int i = 0; i < segmentPoints.Count - 1; i++)
      //      {
      //         LineSegment segment = new LineSegment { StartX = segmentPoints[i].X, StartY = segmentPoints[i].Y, EndX = segmentPoints[i + 1].X, EndY = segmentPoints[i + 1].Y };
      //         edge.Segments.Add(segment);
      //      }
      //   }
      //}

      private static int CalculateNumSegments( Node source, Node dest )
      {
         // Calculate the minimum number of segments needed to connect the source and destination nodes.
         int numSegments = 1;
         int dx = Math.Abs( source.X - dest.X );
         int dy = Math.Abs( source.Y - dest.Y );

         if (dx > 0 && dy > 0)
            numSegments = 2;

         return numSegments;
      }

      private static List<Point> CalculateSegmentPoints( Node source, Node dest, int numSegments )
      {
         // Calculate the points for each segment of the edge.
         List<Point> points = new List<Point> { new Point( source.X, source.Y ) };

         if (numSegments == 1)
         {
            // The nodes are on the same row or column.
            points.Add( new Point( dest.X, dest.Y ) );
         }
         else
         {
            // The nodes are not on the same row or column.
            int dx = Math.Abs( source.X - dest.X );
            int dy = Math.Abs( source.Y - dest.Y );

            int sx = source.X < dest.X
                        ? 1
                        : -1;

            int sy = source.Y < dest.Y
                        ? 1
                        : -1;
            int x = source.X;
            int y = source.Y;
            int err = dx - dy;

            for ( int i = 0; i < numSegments - 1; i++ )
            {
               int e2 = 2 * err;

               if (e2 > -dy)
               {
                  err -= dy;
                  x += sx;
               }

               if (e2 < dx)
               {
                  err += dx;
                  y += sy;
               }

               points.Add( new Point( x, y ) );
            }

            points.Add( new Point( dest.X, dest.Y ) );
         }

         return points;
      }

      public static void Layout( List<Node> nodes, List<Edge> edges, Rectangle? designSurface = null )
      {
         temperature = 100;

         // Calculate the initial size of the design surface based on the positions and sizes of the nodes, if necessary.
         Rectangle surface;

         if (designSurface == null)
         {
            surface = designSurface ?? new Rectangle( int.MaxValue, int.MaxValue, int.MinValue, int.MinValue );

            foreach ( Node node in nodes )
            {
               surface.X = Math.Min( surface.X, node.X );
               surface.Y = Math.Min( surface.Y, node.Y );
               surface.Width = Math.Max( surface.Width, node.X + node.Size.Width );
               surface.Height = Math.Max( surface.Height, node.Y + node.Size.Height );
            }
         }
         else
         {
            surface = designSurface.Value;
         }

         // Calculate the initial positions of the nodes by randomly placing them on the design surface.
         AssignInitialNodePositions( nodes, new Size( surface.Size.Width, surface.Size.Height ) );
         int iterations = 0;

         while ( temperature > 1 && iterations < MaxIterations )
         {
            ApplyForces( nodes );

            //// calculate the force between every pair of nodes
            //for ( int i = 0; i < nodes.Count - 1; i++ )
            //{
            //   for ( int j = i + 1; j < nodes.Count; j++ )
            //   {
            //      Node nodeA = nodes[i];
            //      Node nodeB = nodes[j];
            //      double distance = Math.Sqrt( Math.Pow( nodeB.X - nodeA.X, 2 ) + Math.Pow( nodeB.Y - nodeA.Y, 2 ) );

            //      // calculate Coulomb's law of repulsion
            //      double repulsionForce = CoulombConstant / Math.Pow( distance, 2 );

            //      // add force vector to node A
            //      double repulsionAngle = Math.Atan2( nodeA.Y - nodeB.Y, nodeA.X - nodeB.X );
            //      nodeA.ForceX += repulsionForce * Math.Cos( repulsionAngle );
            //      nodeA.ForceY += repulsionForce * Math.Sin( repulsionAngle );

            //      // add force vector to node B
            //      double attractionForce = HookeConstant * ( distance - optimalDistance );
            //      double attractionAngle = Math.Atan2( nodeB.Y - nodeA.Y, nodeB.X - nodeA.X );
            //      nodeB.ForceX += attractionForce * Math.Cos( attractionAngle );
            //      nodeB.ForceY += attractionForce * Math.Sin( attractionAngle );
            //   }
            //}

            //// update node positions based on the calculated forces
            //foreach ( Node node in nodes )
            //{
            //   double deltaX = node.ForceX * timeStep;
            //   double deltaY = node.ForceY * timeStep;

            //   // limit maximum displacement per iteration to temperature
            //   double displacement = Math.Sqrt( deltaX * deltaX + deltaY * deltaY );

            //   if (displacement > temperature)
            //   {
            //      deltaX = temperature * deltaX / displacement;
            //      deltaY = temperature * deltaY / displacement;
            //   }

            //   // update node position
            //   node.Location = new Point( (int) ( node.X + deltaX ), (int) ( node.Y + deltaY ) );
            //}

            // decrease temperature to approach stable state
            temperature *= DampingFactor;
            iterations++;
         }

         //public void Layout(IEnumerable<Node> nodes, IEnumerable<Edge> edges, Size surfaceSize)
         //{
         //   AssignInitialNodePositions(nodes, surfaceSize);
         //   OptimizeNodePositions(nodes, edges, surfaceSize, 200);
         //   OptimizeNodePositions(nodes, edges, surfaceSize, 200);
         //   OptimizeNodePositions(nodes, edges, surfaceSize, 200);
         //}

         //private void OptimizeNodePositions(IEnumerable<Node> nodes, IEnumerable<Edge> edges, Size surfaceSize, int iterations)
         //{
         //   double k = Math.Sqrt(surfaceSize.Height * surfaceSize.Width / nodes.Count());

         //   for (int i = 0; i < iterations; i++)
         //   {
         //      foreach (Node node in nodes)
         //      {
         //         Point displacement = new Point(0, 0);

         //         foreach (Node other in nodes)
         //         {
         //            if (other != node)
         //            {
         //               Point direction = new Point(other.X - node.X, other.Y - node.Y);

         //               double distance = Math.Max(Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y), 0.1);
         //               direction = new Point((int)Math.Round(direction.X / distance), (int)Math.Round(direction.Y / distance));
         //               displacement.X += direction.X * (int)(k * k / distance);
         //               displacement.Y += direction.Y * (int)(k * k / distance);
         //            }
         //         }

         //         foreach (Edge edge in edges)
         //         {
         //            if (edge.Source == node || edge.Destination == node)
         //            {
         //               Point direction;
         //               Node other;

         //               if (edge.Source == node)
         //               {
         //                  other = edge.Destination;
         //                  direction = new Point(node.X - other.X, node.Y - other.Y);
         //               }
         //               else
         //               {
         //                  other = edge.Source;
         //                  direction = new Point(other.X - node.X, other.Y - node.Y);
         //               }

         //               double distance = Math.Max(Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y), 0.1);
         //               direction = new Point((int)Math.Round(direction.X / distance), (int)Math.Round(direction.Y / distance));
         //               displacement.X -= direction.X * (int)(Math.Pow(distance - k, 2) / k);
         //               displacement.Y -= direction.Y * (int)(Math.Pow(distance - k, 2) / k);
         //            }
         //         }

         //         node.Location = new Point(node.X + displacement.X, node.Y + displacement.Y);
         //         node.Location = new Point(Math.Max(0, Math.Min(surfaceSize.Width, node.X)), Math.Max(0, Math.Min(surfaceSize.Height, node.Y)));
         //      }
         //   }
         //}
      }
   }
}

