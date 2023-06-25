namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   /// Defines an interface for classes that display warning messages.
   /// </summary>
   public interface IDisplaysWarning
   {
      /// <summary>
      /// Gets the value indicating whether the object has warning.
      /// </summary>
      /// <returns>Returns true if has warning, otherwise false.</returns>
      bool GetHasWarningValue();

      /// <summary>
      /// Redraws the item.
      /// </summary>
      void RedrawItem();

      /// <summary>
      /// Resets the warning flag.
      /// </summary>
      void ResetWarning();
   }
}