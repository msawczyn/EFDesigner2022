using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Diagrams;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   /// Represents an interface for handling mouse interactions with compartment shapes.
   /// </summary>
   public interface ICompartmentShapeMouseTarget

   {
      /// <summary>
      /// Moves the compartment item from the specified model element.
      /// </summary>
      /// <param name="dragFrom">The model element to drag the compartment item.</param>
      /// <param name="e">The mouse event arguments of the drag.</param>
      void MoveCompartmentItem(ModelElement dragFrom, DiagramMouseEventArgs e);
   }
}