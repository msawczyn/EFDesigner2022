using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Diagrams;

namespace Sawczyn.EFDesigner.EFModel
{
   public partial class CommentConnector : IHasStore, IThemeable
   {
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