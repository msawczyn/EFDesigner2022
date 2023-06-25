using System.Drawing;
using System.Drawing.Drawing2D;
// ReSharper disable UnusedMemberInSuper.Global

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   ///    Defines the contract for highlighting elements based on selection in the Model Explorer.
   /// </summary>
   public interface IHighlightFromModelExplorer
   {
      /// <summary>
      ///    Gets or sets the color of the outline.
      /// </summary>
      Color OutlineColor { get; set; }

      /// <summary>
      ///    Gets or sets the dash style used for outlining.
      /// </summary>
      DashStyle OutlineDashStyle { get; set; }

      /// <summary>
      ///    Gets or sets the thickness of the outline.
      /// </summary>
      float OutlineThickness { get; set; }

      /// <summary>
      ///    Gets or sets a value indicating whether the object is visible.
      /// </summary>
      bool Visible { get; set; }
   }
}