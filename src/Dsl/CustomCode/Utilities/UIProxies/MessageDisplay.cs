namespace Sawczyn.EFDesigner
{
   /// <summary>
   ///    This helps keep UI interaction out of our DSL project proper. DslPackage calls RegisterDisplayHandler with a method that shows the MessageBox
   ///    (or other UI-related method) properly using the Visual Studio service provider.
   /// </summary>
   public static class MessageDisplay
   {
      private static MessageVisualizer MessageVisualizerMethod;

      ///<summary>
      /// Delegate that defines a method to visualize a message.
      ///</summary>
      ///<param name="message">The message to be visualized</param>
      public delegate void MessageVisualizer(string message);

      /// <summary>
      /// Registers the given message visualizer as the handler for displaying messages.
      /// </summary>
      /// <param name="method">The message visualizer to be registered.</param>
      public static void RegisterDisplayHandler(MessageVisualizer method)
      {
         MessageVisualizerMethod = method;
      }

      /// <summary>
      /// Displays the given message.
      /// </summary>
      /// <param name="message">The message to be displayed.</param>
      public static void Show(string message)
      {
         if (MessageVisualizerMethod != null)
         {
            try
            {
               MessageVisualizerMethod(message);
            }
            catch
            {
               // swallow the exception
            }
         }
      }
   }
}