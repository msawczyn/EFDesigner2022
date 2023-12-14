using System;
using System.Drawing;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   ///    Extension methods for System.Drawing.Color
   /// </summary>
   public static class ColorExtensions
   {
      ///<summary>Returns true if the color is considered dark, otherwise false.</summary>
      public static bool IsDark(this Color color)
      {
         // Counting the perceptive luminance - human eye favors green color... 
         double a = 1 - (((0.299 * color.R) + (0.587 * color.G) + (0.114 * color.B)) / 255);

         // bright colors (a < 0.5)
         // dark colors (a >= 0.5)

         return a >= 0.5;
      }

///<summary>Returns true if the color is considered light, otherwise false.</summary>
public static bool IsLight(this Color color)
      {
         return !color.IsDark();
      }

      /// <summary>
      ///    Calculates a readable color for text against a given background
      /// </summary>
      /// <param name="color">The background color</param>
      public static Color LegibleTextColor(this Color color)
      {
         return color.IsLight()
                   ? Color.Black
                   : Color.White;
      }

      /// <summary>
      ///   Calculates a washed-out version of the given color. Result will be the color halfway between the original and white.
      /// </summary>
      /// <param name="originalColor">The starting color</param>
      /// <returns>Its washed out version</returns>
      public static Color WashedOut(this Color originalColor)
      {
         // Calculate the washed-out color by averaging the RGB values
         int washedOutRed = (originalColor.R + 255) / 2;
         int washedOutGreen = (originalColor.G + 255) / 2;
         int washedOutBlue = (originalColor.B + 255) / 2;

         // Ensure the values are within the valid color range (0-255)
         washedOutRed = Math.Min(255, washedOutRed);
         washedOutGreen = Math.Min(255, washedOutGreen);
         washedOutBlue = Math.Min(255, washedOutBlue);

         // Create and return the washed-out color
         Color washedOutColor = Color.FromArgb(washedOutRed, washedOutGreen, washedOutBlue);
         return washedOutColor;
      }
   }
}