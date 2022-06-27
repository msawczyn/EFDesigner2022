namespace Sawczyn.EFDesigner.EFModel
{
   public interface IDisplaysWarning
   {
      bool GetHasWarningValue();

      void RedrawItem();

      void ResetWarning();
   }
}