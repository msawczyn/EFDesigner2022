using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   /// Represents a form for adding code.
   /// </summary>
   public partial class AddCodeForm : Form
   {
      /// <summary>
      /// Default constructor for the AddCodeForm class
      /// </summary>
      public AddCodeForm()
      {
         InitializeComponent();
      }

      /// <summary>
      /// Initializes a new instance of the AddCodeForm class.
      /// </summary>
      /// <param name="element">An element of type ModelClass.</param>
      public AddCodeForm(ModelClass element) : this()
      {
         lblClassName.Text = element.Name;
         txtCode.Lines = element.Attributes.Select(x => $"{x};").ToArray();
         txtCode.AutoCompleteCustomSource.AddRange(element.ModelRoot.ValidTypes);
      }

      /// <summary>
      /// Constructor for the AddCodeForm class that takes in a ModelEnum parameter.
      /// </summary>
      /// <param name="element">The ModelEnum element to be passed as a parameter.</param>
      public AddCodeForm(ModelEnum element) : this()
      {
         lblClassName.Text = element.Name;
         txtCode.Lines = element.Values.Select(x => x.ToString()).ToArray();

         // ReSharper disable once VirtualMemberCallInConstructor
         Text = "Add values as code";
         label1.Text = "Enum name";
         label2.Text = "Values";
         txtCode.AutoCompleteCustomSource.AddRange(element.ModelRoot.ValidTypes);
      }

      /// <summary>
      /// Gets the lines of text from the file.
      /// </summary>
      public IEnumerable<string> Lines

      {
         get
         {
            return txtCode.Lines.Where(s => !string.IsNullOrEmpty(s.Trim())).ToList();
         }
      }

      /// <summary>
      /// Event handler for the Click event of the btnOk button.
      /// </summary>
      /// <param name="sender">The object that raised the event.</param>
      /// <param name="e">The event data.</param>
      private void btnOk_Click(object sender, EventArgs e)
      {
         DialogResult = DialogResult.OK;
      }
   }
}