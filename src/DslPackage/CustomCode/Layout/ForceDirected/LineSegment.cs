namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   ///    Straight line segment component of an edge
   /// </summary>
   public class LineSegment
   {
      /// <summary>
      ///    X coordinate of the beginning of this segment
      /// </summary>
      public double BeginX { get; set; }

      /// <summary>
      ///    Y coordinate of the beginning of this segment
      /// </summary>
      public double BeginY { get; set; }

      /// <summary>
      ///    X coordinate of the end of this segment
      /// </summary>
      public double EndX { get; set; }

      /// <summary>
      ///    Y coordinate of the end of this segment
      /// </summary>
      public double EndY { get; set; }
   }
}