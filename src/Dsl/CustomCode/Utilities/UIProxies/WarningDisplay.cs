namespace Sawczyn.EFDesigner
{
   /// <summary>
   ///    This helps keep UI interaction out of our DSL project proper. DslPackage calls RegisterDisplayHandler with a method that shows the MessageBox
   ///    (or other UI-related method) properly using the Visual Studio service provider.
   /// </summary>
   public static class WarningDisplay
   {
      private static WarningVisualizer WarningVisualizerMethod;

      /// <summary>
      /// Defines a delegate that represents a warning visualizer method, which takes a string message parameter.
      /// </summary>
      /// <param name="message">The message to be shown as warning.</param>
      public delegate void WarningVisualizer(string message);

      /// <summary>
      /// Registers a delegate to handle the display of warnings
      /// </summary>
      /// <param name="method">The delegate to register</param>
      public static void RegisterDisplayHandler(WarningVisualizer method)
      {
         WarningVisualizerMethod = method;
      }

      /// <summary>
      /// Displays a message to the user
      /// </summary>
      /// <param name="message">The message to be displayed</param>
      public static void Show(string message)
      {
         if (WarningVisualizerMethod != null)
         {
            try
            {
               WarningVisualizerMethod(message);
            }
            catch
            {
               // swallow the exception
            }
         }
      }
   }
}