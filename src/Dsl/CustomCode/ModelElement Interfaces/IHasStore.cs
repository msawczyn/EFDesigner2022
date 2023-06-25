using Microsoft.VisualStudio.Modeling;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   /// Represents an interface for objects that have a store.
   /// </summary>
   public interface IHasStore
   {
      /// <summary>
      /// Gets the Store property.
      /// </summary>
      Store Store { get; }
   }
}