using System;

namespace Sawczyn.EFDesigner
{
   /// <summary>
   ///    This helps keep UI interaction out of our DSL project proper. DslPackage calls RegisterDisplayHandler with a method that shows the MessageBox
   ///    (or other UI-related method) properly using the Visual Studio service provider.
   /// </summary>
   public static class BooleanQuestionDisplay
   {
      private static QuestionVisualizer QuestionVisualizerMethod;

      /// <summary>
      /// Defines a delegate that represents a method that displays a visual question to the user.
      /// </summary>
      /// <param name="serviceProvider">Represents the service provider.</param>
      /// <param name="message">Represents the message to be displayed to the user.</param>
      /// <returns>True, if the user confirms the question; otherwise, false.</returns>
      public delegate bool QuestionVisualizer(IServiceProvider serviceProvider, string message);

      /// <summary>
      /// Registers a display handler for a question visualizer.
      /// </summary>
      /// <param name="method">The method to be registered as a display handler.</param>
      public static void RegisterDisplayHandler(QuestionVisualizer method)
      {
         QuestionVisualizerMethod = method;
      }

      /// <summary>
      /// Displays a message to the user and returns a boolean value.
      /// </summary>
      /// <param name="serviceProvider">A service provider that provides environment services.</param>
      /// <param name="message">The message to display.</param>
      /// <returns>Nullable boolean value. Returns true if the user clicks the "Yes" button, false if the user clicks the "No" button, and null if the user clicks the "Cancel" button.</returns>
      public static bool? Show(IServiceProvider serviceProvider, string message)
      {
         if (QuestionVisualizerMethod != null)
         {
            try
            {
               return QuestionVisualizerMethod(serviceProvider, message);
            }
            catch
            {
               return null;
            }
         }

         return null;
      }
   }
}