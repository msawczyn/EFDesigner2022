using System.Collections.Generic;

namespace Sawczyn.EFDesigner.EFModel
{
   public partial class Generalization : IHasStore
   {
      /// <summary>
      /// Returns the text to display for this object.
      /// </summary>
      public string GetDisplayText()
      {
         return $"{Subclass.Name} inherits from {Superclass.Name}";
      }

      private string GetNameValue()
      {
         return GetDisplayText();
      }

      /// <summary>
      /// Determines if the current type is involved in circular inheritance.
      /// </summary>
      /// <returns>
      /// True if the current type is in a circular inheritance chain, false otherwise.
      /// </returns>
      public bool IsInCircularInheritance()
      {
         List<ModelClass> classes = new List<ModelClass>();

         for (ModelClass modelClass = Subclass; modelClass != null; modelClass = modelClass.Superclass)
         {
            if (classes.Contains(modelClass))
               return true;

            classes.Add(modelClass);
         }

         return false;
      }
   }
}