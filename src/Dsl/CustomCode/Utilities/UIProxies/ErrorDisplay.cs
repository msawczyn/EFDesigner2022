using System;

namespace Sawczyn.EFDesigner
{
   /// <summary>
   ///    This helps keep UI interaction out of our DSL project proper. DslPackage calls RegisterDisplayHandler with a method that shows the MessageBox
   ///    (or other UI-related method) properly using the Visual Studio service provider.
   /// </summary>
   public static class ErrorDisplay
   {
      private static ErrorVisualizer ErrorVisualizerMethod;

      public delegate void ErrorVisualizer(IServiceProvider serviceProvider, string message);

      public static void RegisterDisplayHandler(ErrorVisualizer method)
      {
         ErrorVisualizerMethod = method;
      }

      public static void Show(IServiceProvider serviceProvider, string message)
      {
         if (ErrorVisualizerMethod != null)
         {
            try
            {
               ErrorVisualizerMethod(serviceProvider, message);
            }
            catch
            {
               // swallow the exception
            }
         }
      }
   }
}