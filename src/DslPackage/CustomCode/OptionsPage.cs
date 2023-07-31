using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;

using Microsoft.VisualStudio.Shell;

namespace Sawczyn.EFDesigner.EFModel.DslPackage
{
   /// <summary>
   /// Represents an options page that can be displayed in the Visual Studio Options dialog.
   /// </summary>
   public class OptionsPage : DialogPage
   {
      private string dotExePath;
      private bool saveDiagramsCompressed;
      private bool restrictPropertyTypes = true;

      /// <summary>
      /// Gets or sets a value indicating whether to restrict the property types for the selected fields.
      /// </summary>
      [Category("Designer")]
      [DisplayName("Restrict property types")]
      [Description("If true, restrict property types to built-in types or types defined in the model. If false, any type can be used but there will be no validation that it will work.")]
      public bool RestrictPropertyTypes
      {
         get
         {
            return restrictPropertyTypes;
         }
         set
         {
            OptionsEventArgs args = new OptionsEventArgs("RestrictPorpertyTypes", restrictPropertyTypes, value);
            restrictPropertyTypes = value;
            OnOptionsChanged(args);
         }
      }

      /// <summary>
      /// Gets or sets the path to the dot.exe executable for Graphviz.
      /// </summary>
      [Category("Display")]
      [DisplayName("GraphViz dot.exe path")]
      [Description("Path to the GraphViz dot.exe file (including 'dot.exe'), if present")]
      [Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
      public string DotExePath 
      {
         get
         {
            return dotExePath;
         }
         set
         {
            OptionsEventArgs args = new OptionsEventArgs("DotExePath", dotExePath, value);
            dotExePath = value;
            OnOptionsChanged(args);
         }
      }

      /// <summary>
      /// Saves the diagrams in compressed format.
      /// </summary>
      [Category("File")]
      [DisplayName("Save diagram using legacy binary format")]
      [Description("If true, .diagramx files will be saved in compressed format. If false, they will not.")]
      public bool SaveDiagramsCompressed
      {
         get
         {
            return saveDiagramsCompressed;
         }
         set
         {
            OptionsEventArgs args = new OptionsEventArgs("SaveDiagramsCompressed", saveDiagramsCompressed, value);
            saveDiagramsCompressed = value;
            OnOptionsChanged(args);
         }
      }

      /// <summary>
      /// Raises the OptionsChanged event with the specified arguments.
      /// </summary>
      /// <param name="args">The OptionsEventArgs instance containing the event data.</param>
      protected virtual void OnOptionsChanged(OptionsEventArgs args)
      {
         OptionChanged?.Invoke(this, args);
      }

      /// <summary>
      /// Occurs when an option is changed.
      /// </summary>
      public event EventHandler<OptionsEventArgs> OptionChanged;
   }

   /// <summary>
   /// Provides data for the OptionsChanged event.
   /// </summary>
   public class OptionsEventArgs : EventArgs
   {
      /// <summary>
      /// Initializes a new instance of the OptionsEventArgs class.
      /// </summary>
      /// <param name="option">The option that was changed.</param>
      /// <param name="oldValue">The old value of the option.</param>
      /// <param name="newValue">The new value of the option.</param>
      public OptionsEventArgs(string option, object oldValue, object newValue)
      {
         Option = option;
         OldValue = oldValue;
         NewValue = newValue;
      }

      /// <summary>
      /// Gets the Option value
      /// </summary>
      public string Option { get; }
      /// <summary>
      /// Gets the old value.
      /// </summary>
      public object OldValue { get; }
      /// <summary>
      /// Gets the new value of the object.
      /// </summary>
      public object NewValue { get; }
   }
}