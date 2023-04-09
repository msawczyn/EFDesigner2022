public class Node
{
	// list of references to connected nodes (children)
	private readonly List<Node> _connections
	public IList<Node> Connections
	{
		get => mConnections.AsReadOnly();
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
	
   /// <summary>
   ///    Connects the specified child node to this node.
   /// </summary>
   /// <param name="child">The child node to add.</param>
   /// <returns>True if the node was connected to this node.</returns>
   public bool AddChild( Node child )
   {
      if (child == null)
         throw new ArgumentNullException( nameof( child ) );

      if (child != this && !mConnections.Contains( child ))
      {
         mConnections.Add( child );
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
      bool c = mConnections.Remove( other );
      bool p = other.mConnections.Remove( this );
      return c || p;
   }
	
	public Node()
	{
		mLocation = Point.Empty;
		mConnections = new List<Node>();
	}
}
