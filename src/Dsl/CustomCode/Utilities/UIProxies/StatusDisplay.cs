namespace Sawczyn.EFDesigner
{
   public static class StatusDisplay
   {
      private static StatusVisualizer StatusVisualizerMethod;

      public delegate void StatusVisualizer(string message);

      public static void RegisterDisplayHandler(StatusVisualizer method)
      {
         StatusVisualizerMethod = method;
      }

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