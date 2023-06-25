using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Diagrams;

namespace Sawczyn.EFDesigner.EFModel
{
   public partial class CommentConnector : IHasStore, IThemeable
   {
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
      /// Modifies the current luminosity of the connector in a diagram client's view. 
      /// </summary>
      /// <param name="currentLuminosity">The current luminosity of the connector.</param>
      /// <param name="view">The diagram client's view.</param>
      /// <returns>The modified luminosity of the connector.</returns>
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