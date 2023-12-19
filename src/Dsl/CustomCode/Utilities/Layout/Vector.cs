using System;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   ///    2D vector from (0,0) to (X,Y)
   /// </summary>
   public class Vector
   {
      /// <summary>
      ///    Constructor
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      public Vector(double x, double y)
      {
         X = x;
         Y = y;
      }

      /// <summary>
      ///    X coordinate
      /// </summary>
      public double X { get; set; }

      /// <summary>
      ///    Y coordinate
      /// </summary>
      public double Y { get; set; }

      /// <summary>
      ///    Length of the vector
      /// </summary>
      public double Length()
      {
         return (double)Math.Sqrt(LengthSquared());
      }

      /// <summary>
      ///    Length of the vector squared
      /// </summary>
      public double LengthSquared()
      {
         return X * X + Y * Y;
      }

      /// <summary>
      ///    Add two vectors. Creates a new vector from the sum.
      /// </summary>
      /// <param name="a">First vector to add</param>
      /// <param name="b">Second vector to add</param>
      /// <returns></returns>
      public static Vector operator +(Vector a, Vector b)
      {
         return new Vector(a.X + b.X, a.Y + b.Y);
      }

      /// <summary>
      ///    Divide a vector by a scalar. Creates a new vector from the quotient.
      /// </summary>
      /// <param name="vector">Vector to modify</param>
      /// <param name="scalar">Scalar divisor</param>
      public static Vector operator /(Vector vector, double scalar)
      {
         return new Vector(vector.X / scalar, vector.Y / scalar);
      }

      /// <summary>
      ///    Multiply a vector by a scalar. Creates a new vector from the product.
      /// </summary>
      /// <param name="vector">Vector to modify</param>
      /// <param name="scalar">Scalar multiplier</param>
      public static Vector operator *(Vector vector, double scalar)
      {
         return new Vector(vector.X * scalar, vector.Y * scalar);
      }

      /// <summary>
      ///    Subtract two vectors. Creates a new vector from the difference.
      /// </summary>
      /// <param name="a">Starting vector</param>
      /// <param name="b">Vector to subtract</param>
      public static Vector operator -(Vector a, Vector b)
      {
         return new Vector(a.X - b.X, a.Y - b.Y);
      }
   }
}