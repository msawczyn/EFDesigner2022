using System.Drawing;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   /// Represents the colors of a diagram theme.
   /// </summary>
   public class DiagramThemeColors
   {
      /// <summary>
      /// Initializes a new instance of the DiagramThemeColors class with a specified background color.
      /// </summary>
      /// <param name="background">The background color to set.</param>
      public DiagramThemeColors(Color background)
      {
         Background = background;
      }

      /// <summary>
      /// Gets the background color.
      /// </summary>
      public Color Background { get; }

      /// <summary>
      /// Gets or sets the color of the text.
      /// </summary>
      public Color Text 
      {
         get
         {
            return Background.LegibleTextColor();
         }
      }

      /// <summary>
      /// Gets or sets the background color of the header.
      /// </summary>
      public Color HeaderBackground 
      {
         get
         {
            return Background.IsDark()
                      ? Color.Gray
                      : Color.LightGray;
         }
      }

      /// <summary>
      /// Gets or sets the color of the header text.
      /// </summary>
      public Color HeaderText
      {
         get
         {
            return HeaderBackground.LegibleTextColor();
         }
      }
   }
}