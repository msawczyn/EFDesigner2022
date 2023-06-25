namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   ///    Defines the contract for an object that can be themed.
   /// </summary>
   public interface IThemeable
   {
      /// <summary>
      ///    Sets the colors of the theme for the specified diagram
      /// </summary>
      /// <param name="diagramColors">The colors to set for the theme of the diagram</param>
      void SetThemeColors(DiagramThemeColors diagramColors);
   }
}