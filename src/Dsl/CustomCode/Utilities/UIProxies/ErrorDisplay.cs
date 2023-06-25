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

      /// <summary>
      /// Represents a delegate that visualizes errors.
      /// </summary>
      /// <param name="serviceProvider">The service provider.</param>
      /// <param name="message">The error message to visualize.</param>
      public delegate void ErrorVisualizer(IServiceProvider serviceProvider, string message);

      /// <summary>
      /// Registers a display handler for errors.
      /// </summary>
      /// <param name="method">The method to register.</param>
      public static void RegisterDisplayHandler(ErrorVisualizer method)
      {
         ErrorVisualizerMethod = method;
      }

      /// <summary>
      /// Displays a message using the specified service provider.
      /// </summary>
      /// <param name="serviceProvider">The service provider used to display the message.</param>
      /// <param name="message">The message to display.</param>
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