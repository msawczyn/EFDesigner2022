namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   ///    Extension methods for Sawczyn.EFDesigner.EFModel.Multiplicity
   /// </summary>
   public static class MultiplicityExtensions
   {
      /// <summary>
      /// Formats the specified Multiplicity value as a string suitable for display.
      /// </summary>
      /// <param name="value">The value to display.</param>
      /// <returns>A string representation of the Multiplicity value.</returns>
      public static string Display(this Multiplicity value)
      {
         switch (value)
         {
            case Multiplicity.One:
               return "1";

            case Multiplicity.ZeroOne:
               return "0..1";

            case Multiplicity.ZeroMany:
               return "0..*";

            default:
               return string.Empty;
         }
      }
   }
}