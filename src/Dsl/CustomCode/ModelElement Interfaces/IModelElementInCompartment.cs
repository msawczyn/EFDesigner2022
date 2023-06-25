namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   ///    Tag interface indicating diagram items for this element are compartments in a parent element
   /// </summary>
   public interface IModelElementInCompartment
   {
      ///<summary>
      ///Gets the parent model element of the current model element.
      ///</summary>
      IModelElementWithCompartments ParentModelElement { get; }
      /// <summary>
      /// Gets the name of the compartment.
      /// </summary>
      string CompartmentName { get; }
   }
}