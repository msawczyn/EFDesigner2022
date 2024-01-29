using System;

using Microsoft.VisualStudio.TextTemplating;

// ReSharper disable UnusedMember.Global

namespace Sawczyn.EFDesigner.EFModel.EditingOnly
{
   // ReSharper disable once UnusedMember.Global
   public partial class GeneratedTextTransformation
   {
      // stubs for methods provided by the EFModelDirectiveProcessor

      /// <summary>
      /// Gets or sets the root model object.
      /// </summary>
      public ModelRoot ModelRoot { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="ITextTemplatingEngineHost"/> used by the text templating engine.
      /// </summary>
      public ITextTemplatingEngineHost Host { get; set; }

      /// <summary>
      /// Clears the indentation.
      /// </summary>
      public void ClearIndent() { }

      /// <summary>
      /// Removes one indentation level and returns an empty string.
      /// </summary>
      public string PopIndent() { return string.Empty; }

      /// <summary>
      /// Increments the indent level.
      /// </summary>
      /// <param name="indent">The string to add to the current indent level.</param>
      public void PushIndent(string indent) { }

      /// <summary>
      /// Writes a line of text to the output.
      /// </summary>
      /// <param name="textToAppend">The text to be written.</param>
      public void WriteLine(string textToAppend) { }

      #region Template

      // EFDesigner v4.2.8.1
      // Copyright (c) 2017-2023 Michael Sawczyn
      // https://github.com/msawczyn/EFDesigner

      /// <summary>
      /// Generates Entity Framework 6 classes based on manager and modelRoot objects
      /// </summary>
      /// <param name="manager">Manager object</param>
      /// <param name="modelRoot">ModelRoot object</param>
      public void GenerateEF6(Manager manager, ModelRoot modelRoot)
      {
         if (modelRoot.EntityFrameworkVersion != EFVersion.EF6)
            throw new InvalidOperationException("Wrong generator selected");

         EFModelGenerator generator = new EF6ModelGenerator(this);
         generator.Generate(manager);
      }

      /// <summary>
      /// Generates Entity Framework Core classes based on the manager and model root provided
      /// </summary>
      /// <param name="manager">The manager containing the data source information</param>
      /// <param name="modelRoot">The model root containing the entity information</param>
      public void GenerateEFCore(Manager manager, ModelRoot modelRoot)
      {
         if (modelRoot.EntityFrameworkVersion != EFVersion.EFCore)
            throw new InvalidOperationException("Wrong generator selected");

         EFModelGenerator generator;

         switch ((int)modelRoot.GetEntityFrameworkPackageVersionNum())
         {
            case 2:
               generator = new EFCore2ModelGenerator(this);

               break;

            case 3:
               generator = new EFCore3ModelGenerator(this);

               break;

            default:
               generator = new EFCore5ModelGenerator(this);

               break;
         }

         generator.Generate(manager);
      }

#endregion Template
   }
}