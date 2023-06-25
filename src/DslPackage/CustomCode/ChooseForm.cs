using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   /// Represents a form used for selecting an option from a list of choices.
   /// </summary>
   public partial class ChooseForm : Form
   {
      /// <summary>
      /// Initializes a new instance of the ChooseForm class.
      /// </summary>
      public ChooseForm()
      {
         InitializeComponent();
      }

      /// <summary>
      /// Gets or sets the title of the document.
      /// </summary>
      public string Title 
      {
         get { return lblTitle.Text; }
         set { lblTitle.Text = value; }
      }

      /// <summary>
      /// Gets or sets the selected value
      /// </summary>
      public string Selection
      {
         get { return lbChoices.Text; }
      }

      private void LbChoices_SelectedIndexChanged(object sender, EventArgs e)
      {
         btnOK.Enabled = lbChoices.SelectedIndex >= 0;
      }

      /// <summary>
      /// Sets the choices for a given question
      /// </summary>
      /// <param name="choices">A collection of strings representing the choices</param>
      public void SetChoices(IEnumerable<string> choices)
      {
         lbChoices.Items.Clear();

         if (choices == null)
            return;

         // ReSharper disable once CoVariantArrayConversion
         lbChoices.Items.AddRange(choices.ToArray());
      }
   }
}