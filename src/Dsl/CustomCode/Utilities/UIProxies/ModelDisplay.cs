using System;

using Sawczyn.EFDesigner.EFModel;

namespace Sawczyn.EFDesigner
{
   /// <summary>
   ///    This helps keep UI interaction out of our DSL project proper. DslPackage calls the various methods to register handlers at the UI level.
   /// </summary>
   public static class ModelDisplay
   {
      /// <summary>
      /// Gets the diagram theme colors.
      /// </summary>
      public static Func<DiagramThemeColors> GetDiagramColors;

      private static LayoutDiagramAction LayoutDiagramMethod;

      /// <summary>
      ///   Executes autolayout on the diagram passed in
      /// </summary>
      /// <param name="diagram">EFModelDiagram instance to perform action upon.</param>
      public delegate void LayoutDiagramAction(EFModelDiagram diagram);

      /// <summary>
      /// Applies a layout algorithm to the given EFModelDiagram object
      /// </summary>
      /// <param name="diagram">The EFModelDiagram object to be laid out</param>
      public static void LayoutDiagram(EFModelDiagram diagram)
      {
         if (LayoutDiagramMethod != null)
         {
            try
            {
               LayoutDiagramMethod(diagram);
            }
            catch
            {
               // swallow the exception
            }
         }
      }

      /// <summary>
      /// Registers a LayoutDiagramAction method.
      /// </summary>
      /// <param name="method">The LayoutDiagramAction method to register.</param>
      public static void RegisterLayoutDiagramAction(LayoutDiagramAction method)
      {
         LayoutDiagramMethod = method;
      }
   }
}