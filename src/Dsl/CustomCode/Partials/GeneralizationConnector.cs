using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Diagrams;

namespace Sawczyn.EFDesigner.EFModel
{
   public partial class GeneralizationConnector : IHasStore, IThemeable
   {
      /// <summary>
      /// Get/Set whether or not the Shape shows a mouse hover tooltip by default
      /// </summary>
      public override bool HasToolTip
      {
         get
         {
            return true;
         }
      }

      /// <summary>
      /// Sets this object's colors in the diagram, based on the current theme
      /// </summary>
      /// <param name="diagramColors">The colors to use.</param>
      public void SetThemeColors(DiagramThemeColors diagramColors)
      {
         Transaction tx = Store.TransactionManager.InTransaction
                             ? null
                             : Store.TransactionManager.BeginTransaction("Set diagram colors");
         try
         {
            Color = diagramColors.Background.LegibleTextColor();
            TextColor = diagramColors.Text;
         }
         finally
         {
            if (tx != null)
            {
               tx.Commit();
               tx.Dispose();
            }
         }
      }

      /// <summary>
      /// Gets the tooltip text for the PEL element under the cursor
      /// </summary>
      /// <param name="item">this contains the shape,field, and subfield under the cursor</param>
      /// <returns></returns>
      public override string GetToolTipText(DiagramItem item)
      {
         return item.Shape.ModelElement is Generalization generalization
                   ? $"{generalization.Subclass.Name} inherits from {generalization.Superclass.Name}"
                   : string.Empty;
      }

      /// <summary>
      /// Calculates highlight luminosity based on:
      /// 	if L &gt;= 160, then L = L * 0.9
      /// 	else, L += 40.
      /// </summary>
      /// <param name="currentLuminosity">The current luminosity of the element.</param>
      /// <param name="view">The client view of the diagram.</param>
      /// <returns>Returns the modified luminosity value.</returns>
      protected override int ModifyLuminosity(int currentLuminosity, DiagramClientView view)
      {
         if (!view.HighlightedShapes.Contains(new DiagramItem(this)))
            return currentLuminosity;

         int baseCalculation = base.ModifyLuminosity(currentLuminosity, view);

         // black (luminosity == 0) will be changed to luminosity 40, which doesn't show up.
         // so if it's black we're highlighting, return 130, since that looks ok.
         return baseCalculation == 40
                   ? 130
                   : baseCalculation;
      }

      /// <summary>
      ///    This method is called when a shape is inititially created, derived classes can
      ///    override to perform shape instance initialization.  This method is always called within a transaction.
      /// </summary>
      public override void OnInitialize()
      {
         base.OnInitialize();

         if (ModelDisplay.GetDiagramColors != null)
            SetThemeColors(ModelDisplay.GetDiagramColors());
      }
   }
}