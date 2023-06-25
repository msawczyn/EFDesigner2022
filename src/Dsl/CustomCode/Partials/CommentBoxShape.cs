using System.Collections.Generic;

using Microsoft.VisualStudio.Modeling.Diagrams;

namespace Sawczyn.EFDesigner.EFModel
{
   public partial class CommentBoxShape : IHasStore, IThemeable
   {
      /// <summary>
      /// Sets this object's colors in the diagram, based on the current theme
      /// </summary>
      /// <param name="diagramColors">The colors to use.</param>
      public void SetThemeColors(DiagramThemeColors diagramColors)
      {
         //using (Transaction tx = Store.TransactionManager.BeginTransaction("Set diagram colors"))
         //{
         //   FillColor = diagramColors.Background;
         //   TextColor = FillColor.LegibleTextColor();

         //   Invalidate();

         //   tx.Commit();
         //}
      }

      /// <summary>
      /// Initializes decorator objects for the specified shape fields.
      /// </summary>
      /// <param name="shapeFields">List of shape fields.</param>
      /// <param name="decorators">List of decorators to initialize.</param>
      protected override void InitializeDecorators(IList<ShapeField> shapeFields, IList<Decorator> decorators)
      {
         //Called once for each shape instance. 
         base.InitializeDecorators(shapeFields, decorators);

         //Look up the shape field, which is called "Comment." 
         TextField commentField = (TextField)FindShapeField(shapeFields, "Comment");

         // Allow multiple lines of text. 
         commentField.DefaultMultipleLine = true;

         // Autosize not supported for multi-line fields. 
         commentField.DefaultAutoSize = false;

         // Anchor the field slightly inside the container shape. 
         commentField.AnchoringBehavior.Clear();
         commentField.AnchoringBehavior.SetLeftAnchor(AnchoringBehavior.Edge.Left, 0.01);
         commentField.AnchoringBehavior.SetRightAnchor(AnchoringBehavior.Edge.Right, 0.01);
         commentField.AnchoringBehavior.SetTopAnchor(AnchoringBehavior.Edge.Top, 0.01);
         commentField.AnchoringBehavior.SetBottomAnchor(AnchoringBehavior.Edge.Bottom, 0.01);
      }

      /// <summary>
      ///    Shape instance initialization.
      /// </summary>
      public override void OnInitialize()
      {
         base.OnInitialize();

         if (ModelDisplay.GetDiagramColors != null)
            SetThemeColors(ModelDisplay.GetDiagramColors());
      }
   }
}