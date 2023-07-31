namespace Sawczyn.EFDesigner
{
   /// <summary>
   /// Provides methods to display status messages
   /// </summary>
   public static class StatusDisplay
   {
      private static StatusVisualizer StatusVisualizerMethod;

      /// <summary>
      /// A delegate used to visualize the status with a message.
      /// </summary>
      /// <param name="message">The message to be visualized.</param>
      public delegate void StatusVisualizer(string message);

      /// <summary>
      /// Registers a display handler for the given method.
      /// </summary>
      /// <param name="method">The method that will handle the display</param>
      public static void RegisterDisplayHandler(StatusVisualizer method)
      {
         StatusVisualizerMethod = method;
      }

      /// <summary>
      /// Displays the given message on the console.
      /// </summary>
      /// <param name="message">The message to display.</param>
      public static void Show(string message)
      {
         if (StatusVisualizerMethod != null)
         {
            try
            {
               StatusVisualizerMethod(message);
            }
            catch
            {
               // swallow the exception
            }
         }
      }
   }
}