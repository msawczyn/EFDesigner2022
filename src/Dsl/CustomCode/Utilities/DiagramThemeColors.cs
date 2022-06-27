using System.Drawing;

namespace Sawczyn.EFDesigner.EFModel
{
   public class DiagramThemeColors
   {
      public DiagramThemeColors(Color background)
      {
         Background = background;
      }

      public Color Background { get; }

      public Color Text
      {
         get
         {
            return Background.LegibleTextColor();
         }
      }

      public Color HeaderBackground
      {
         get
         {
            return Background.IsDark()
                      ? Color.Gray
                      : Color.LightGray;
         }
      }

      public Color HeaderText
      {
         get
         {
            return HeaderBackground.LegibleTextColor();
         }
      }
   }
}