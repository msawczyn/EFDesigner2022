using System.Collections.Generic;

namespace Sawczyn.EFDesigner
{
   /// <summary>
   /// A static class that provides methods for displaying the choices available to the user.
   /// </summary>
   public static class ChoiceDisplay

   {
      private static ChoiceVisualizer ChoiceVisualizerMethod;

      /// <summary>
      /// Delegate that defines a method for visualizing a choice with a title and a list of choices.
      /// </summary>
      /// <param name="title">The title of the choice.</param>
      /// <param name="choices">The list of choices</param>
      /// <returns>A string representing the visualized choice.</returns>
      public delegate string ChoiceVisualizer(string title, IEnumerable<string> choices);

      /// <summary>
      /// Gets the user's choice from a list of options provided.
      /// </summary>
      /// <param name="title">The title of the menu or list of options.</param>
      /// <param name="choices">The list of options for the user to choose from.</param>
      /// <returns>The user's choice as a string.</returns>
      public static string GetChoice(string title, IEnumerable<string> choices)
      {
         if (ChoiceVisualizerMethod != null)
         {
            try
            {
               return ChoiceVisualizerMethod(title, choices);
            }
            catch
            {
               return null;
            }
         }

         return null;
      }

      /// <summary>
      /// Registers a method to handle displaying the choices for a question.
      /// </summary>
      /// <param name="method">The method to register.</param>
      public static void RegisterDisplayHandler(ChoiceVisualizer method)
      {
         ChoiceVisualizerMethod = method;
      }
   }
}