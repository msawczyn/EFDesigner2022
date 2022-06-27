namespace Sawczyn.EFDesigner.EFModel
{
   public partial class Comment : IHasStore
   {
      private string GetNameValue()
      {
         return GetShortTextValue();
      }

      private string GetShortTextValue()
      {
         return Text.Truncate(50);
      }
   }
}