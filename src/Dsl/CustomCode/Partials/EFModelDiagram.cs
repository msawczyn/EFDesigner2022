using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Diagrams;
using Microsoft.VisualStudio.Modeling.Diagrams.GraphObject;

using Sawczyn.EFDesigner.EFModel.Extensions;

namespace Sawczyn.EFDesigner.EFModel
{
   public partial class EFModelDiagram : IHasStore, IThemeable
   {
      /// <summary>
      /// The position of the mouse when a MouseDown event occurs.
      /// </summary>
      public PointD MouseDownPosition;

      /// <summary>
      ///    When true, user is dropping a drag from an external (to the model) source
      /// </summary>
      public static bool IsDroppingExternal { get; private set; }

      private bool ForceAddShape { get; set; }

      private List<ModelElement> DisplayedElements
      {
         get
         {
            return NestedChildShapes.Select(nestedShape => nestedShape.ModelElement).ToList();
         }
      }

      /// <summary>
      /// Updates the color theme for a store's diagram
      /// </summary>
      /// <param name="store">The store to update the color theme for</param>
      /// <param name="diagramColors">The colors to use for the diagram</param>
      public static void UpdateColors(Store store, DiagramThemeColors diagramColors)
      {
         if (store != null)
         {
            Transaction tx = store.TransactionManager.InTransaction
                                ? null
                                : store.TransactionManager.BeginTransaction("Set diagram colors");

            try
            {
               foreach (GeneralizationConnector connector in store.GetAll<GeneralizationConnector>().ToArray())
                  connector.SetThemeColors(diagramColors);

               foreach (AssociationConnector connector in store.GetAll<AssociationConnector>().ToArray())
                  connector.SetThemeColors(diagramColors);

               foreach (CommentConnector connector in store.GetAll<CommentConnector>().ToArray())
                  connector.SetThemeColors(diagramColors);

               foreach (ClassShape classShape in store.GetAll<ClassShape>().ToArray())
                  classShape.SetThemeColors(diagramColors);

               foreach (EnumShape enumShape in store.GetAll<EnumShape>().ToArray())
                  enumShape.SetThemeColors(diagramColors);

               foreach (CommentBoxShape commentShape in store.GetAll<CommentBoxShape>().ToArray())
                  commentShape.SetThemeColors(diagramColors);

               foreach (EFModelDiagram diagram in store.GetAll<EFModelDiagram>().ToArray())
                  diagram.SetThemeColors(diagramColors);
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
      }

      /// <summary>
      /// Gets the color scheme for a diagram theme.
      /// </summary>
      /// <returns>A DiagramThemeColors object containing the color scheme.</returns>
      public DiagramThemeColors GetThemeColors()
      {
         return new DiagramThemeColors(FillColor);
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
            FillColor = diagramColors.Background;
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

      private void AddElementsToActiveDiagram(List<ModelElement> newElements)
      {
         ModelElement[] modelElements = newElements.Where(e => e is ModelClass || e is ModelEnum).ToArray();
         int elementCount = modelElements.Length;

         for (int index = 0; index < modelElements.Length; index++)
         {
            ModelElement element = modelElements[index];
            StatusDisplay.Show($"Adding node {index + 1} of {elementCount} to diagram");
            AddExistingModelElement(this, element);
         }
      }

      /// <summary>
      /// Adds an existing model element to the given EFModelDiagram.
      /// </summary>
      /// <param name="diagram">The EFModelDiagram to add the element to.</param>
      /// <param name="element">The ModelElement to add to the diagram.</param>
      /// <returns>The generated ShapeElement.</returns>
      public static ShapeElement AddExistingModelElement(EFModelDiagram diagram, ModelElement element)
      {
         if (diagram?.NestedChildShapes?.Any(s => s.ModelElement == element) != false)
            return null;

         using (Transaction t = element.Store.TransactionManager.BeginTransaction("add existing model elements"))
         {
            diagram.ForceAddShape = true;
            FixUpAllDiagrams.FixUp(diagram, element);
            diagram.ForceAddShape = false;

            // find all element links that are attached to our element where the ends are in the diagram but the link isn't already in the diagram
            List<ElementLink> elementLinks = element.Store.GetAll<ElementLink>()
                                                    .Where(link => link.LinkedElements.Contains(element)
                                                                && link.LinkedElements.All(linkedElement => diagram.DisplayedElements.Contains(linkedElement))
                                                                && !diagram.DisplayedElements.Contains(link))
                                                    .ToList();

            foreach (ElementLink elementLink in elementLinks)
               FixUpAllDiagrams.FixUp(diagram, elementLink);

            t.Commit();

            return diagram.NestedChildShapes.FirstOrDefault(s => s.ModelElement == element);
         }
      }

      /// <summary>
      /// Disables diagram rules.
      /// </summary>
      public void DisableDiagramRules()
      {
         RuleManager ruleManager = Store.RuleManager;
         ruleManager.DisableRule(typeof(FixUpAllDiagrams));
         ruleManager.DisableRule(typeof(DecoratorPropertyChanged));
         ruleManager.DisableRule(typeof(ConnectorRolePlayerChanged));
         ruleManager.DisableRule(typeof(CompartmentItemAddRule));
         ruleManager.DisableRule(typeof(CompartmentItemDeleteRule));
         ruleManager.DisableRule(typeof(CompartmentItemRolePlayerChangeRule));
         ruleManager.DisableRule(typeof(CompartmentItemRolePlayerPositionChangeRule));
         ruleManager.DisableRule(typeof(CompartmentItemChangeRule));
      }

      /// <summary>
      /// Enables the rules for the diagram.
      /// </summary>
      public void EnableDiagramRules()
      {
         RuleManager ruleManager = Store.RuleManager;
         ruleManager.EnableRule(typeof(FixUpAllDiagrams));
         ruleManager.EnableRule(typeof(DecoratorPropertyChanged));
         ruleManager.EnableRule(typeof(ConnectorRolePlayerChanged));
         ruleManager.EnableRule(typeof(CompartmentItemAddRule));
         ruleManager.EnableRule(typeof(CompartmentItemDeleteRule));
         ruleManager.EnableRule(typeof(CompartmentItemRolePlayerChangeRule));
         ruleManager.EnableRule(typeof(CompartmentItemRolePlayerPositionChangeRule));
         ruleManager.EnableRule(typeof(CompartmentItemChangeRule));
      }

      ///<summary>
      ///Highlights the specified ShapeElement.
      ///</summary>
      ///<param name="shape">The ShapeElement to be highlighted.</param>
      public void Highlight(ShapeElement shape)
      {
         ShapeElement highlightShape = DetermineHighlightShape(shape);

         if (highlightShape != null)
            ActiveDiagramView.DiagramClientView.HighlightedShapes.Add(new DiagramItem(highlightShape));
      }

      /// <summary>
      ///    Used for dropping data originating outside of VStudio
      /// </summary>
      /// <param name="diagramDragEventArgs"></param>
      /// <returns></returns>
      private bool IsAcceptableDropItem(DiagramDragEventArgs diagramDragEventArgs)
      {
         IsDroppingExternal = (diagramDragEventArgs.Data.GetData("Text") is string filenames1
                            && filenames1.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries) is string[] filenames2
                            && filenames2.All(File.Exists))
                           || (diagramDragEventArgs.Data.GetData("FileDrop") is string[] filenames3 && filenames3.All(File.Exists));

         bool isDroppingAssociationClass = ClassShape.ClassShapeDragData?.GetBidirectionalConnectorsUnderShape(diagramDragEventArgs.MousePosition).Any() == true;

         return IsDroppingExternal || isDroppingAssociationClass;
      }

      /// <summary>
      /// Event raised when an IDataObject is dragged over and then dropped into the ShapeElement.
      /// </summary>
      /// <param name="diagramDragEventArgs">The drag arguments to be used to update the model with the IDataObject dropped.</param>
      /// <remarks>
      /// A hit test is performed and the event is fired to the most relevant ShapeElement.
      /// </remarks>
      public override void OnDragDrop(DiagramDragEventArgs diagramDragEventArgs)
      {
         ModelElement element = diagramDragEventArgs.Data.GetData("Sawczyn.EFDesigner.EFModel.ModelClass") as ModelElement
                             ?? diagramDragEventArgs.Data.GetData("Sawczyn.EFDesigner.EFModel.ModelEnum") as ModelElement;

         // TODO: This isn't ready for prime time. Need to handle multiple identity properties
         //List<BidirectionalConnector> candidates = ClassShape.ClassShapeDragData?.GetBidirectionalConnectorsUnderShape(diagramDragEventArgs.MousePosition);

         // are we creating an association class?

         //if (candidates?.Any() == true)
         //{
         //   MakeAssociationClass(candidates);

         //   try
         //   {
         //      base.OnDragDrop(diagramDragEventArgs);
         //   }
         //   catch (ArgumentException)
         //   {
         //      // ignore. byproduct of multiple diagrams
         //   }
         //}

         //// came from model explorer?
         //else 
         if (element != null)
            AddToDiagram(element, diagramDragEventArgs.MousePosition);

         else if (IsDroppingExternal)
            DropExternalElements(diagramDragEventArgs);

         else
         {
            try
            {
               base.OnDragDrop(diagramDragEventArgs);
            }
            catch (ArgumentException)
            {
               // ignore. byproduct of multiple diagrams
            }
         }

         StatusDisplay.Show("Ready");
         Invalidate();

         string BuildMessage(List<ModelElement> newElements)
         {
            int classCount = newElements.OfType<ModelClass>().Count();
            int propertyCount = newElements.OfType<ModelClass>().SelectMany(c => c.Attributes).Count();
            int enumCount = newElements.OfType<ModelEnum>().Count();
            List<string> messageParts = new List<string>();

            if (classCount > 0)
               messageParts.Add($"{classCount} classes");

            if (propertyCount > 0)
               messageParts.Add($"{propertyCount} properties");

            if (enumCount > 0)
               messageParts.Add($"{enumCount} enums");

            return $"Import dropped files: added {(messageParts.Count > 1 ? string.Join(", ", messageParts.Take(messageParts.Count - 1)) + " and " + messageParts.Last() : messageParts.First())}";
         }

         //void MakeAssociationClass(List<BidirectionalConnector> possibleConnectors)
         //{
         //   ModelClass modelClass = (ModelClass)ClassShape.ClassShapeDragData.ClassShape.ModelElement;

         //   using (Transaction t = modelClass.Store.TransactionManager.BeginTransaction("Creating association class"))
         //   {
         //      foreach (BidirectionalConnector candidate in possibleConnectors)
         //      {
         //         BidirectionalAssociation association = (BidirectionalAssociation)candidate.ModelElement;

         //         if (BooleanQuestionDisplay.Show(Store, $"Make {modelClass.Name} an association class for {association.GetDisplayText()}?") == true)
         //         {
         //            modelClass.ConvertToAssociationClass(association);
         //            ClassShape.ClassShapeDragData = null;

         //            break;
         //         }
         //      }

         //      t.Commit();
         //   }

         //   ClassShape.ClassShapeDragData = null;
         //}

         void AddToDiagram(ModelElement elementToAdd, PointD atPosition)
         {
            ShapeElement newShape = AddExistingModelElement(this, elementToAdd);

            if (newShape != null)
            {
               using (Transaction t = elementToAdd.Store.TransactionManager.BeginTransaction("Moving pasted shapes"))
               {
                  if (newShape is NodeShape nodeShape)
                     nodeShape.Location = atPosition;

                  t.Commit();
               }
            }
         }

         void DropExternalElements(DiagramDragEventArgs eventData)
         {
            DisableDiagramRules();
            Cursor prev = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            List<ModelElement> newElements = null;

            try
            {
               try
               {
                  // add to the model
                  string[] filenames;

                  if (eventData.Data.GetData("Text") is string concatenatedFilenames)
                     filenames = concatenatedFilenames.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
                  else if (eventData.Data.GetData("FileDrop") is string[] droppedFilenames)
                     filenames = droppedFilenames;
                  else
                  {
                     ErrorDisplay.Show(Store, "Unexpected error dropping files. Please create an issue in Github.");

                     return;
                  }

                  string[] existingFiles = filenames.Where(File.Exists).ToArray();
                  newElements = FileDropHelper.HandleMultiDrop(Store, existingFiles).ToList();

                  string[] missingFiles = filenames.Except(existingFiles).ToArray();

                  if (missingFiles.Any())
                  {
                     if (missingFiles.Length > 1)
                        missingFiles[missingFiles.Length - 1] = "and " + missingFiles[missingFiles.Length - 1];

                     ErrorDisplay.Show(Store, $"Can't find files {string.Join(", ", missingFiles)}");
                  }
               }
               finally
               {
                  if (newElements?.Count > 0)
                  {
                     string message = $"Created {newElements.Count} new elements that have been added to the Model Explorer. "
                                    + "Do you want these added to the current diagram as well? It could take a while.";

                     if (BooleanQuestionDisplay.Show(Store, message) == true)
                        AddElementsToActiveDiagram(newElements);
                  }

                  IsDroppingExternal = false;
               }
            }
            finally
            {
               EnableDiagramRules();
               Cursor.Current = prev;

               MessageDisplay.Show((newElements == null) || !newElements.Any()
                                      ? "Import dropped files: no new elements added"
                                      : BuildMessage(newElements));

               StatusDisplay.Show("Ready");
            }
         }
      }

      /// <summary>
      /// Event raised when an IDataObject is dragged over the ShapeElement's bounds.
      /// </summary>
      /// <param name="diagramDragEventArgs">The drag arguments to be set to indicate how to handle the drag.</param>
      /// <remarks>
      /// A hit test is performed by the DiagramClientView and the event is fired to the most relevant ShapeElement.
      /// </remarks>
      public override void OnDragOver(DiagramDragEventArgs diagramDragEventArgs)
      {
         base.OnDragOver(diagramDragEventArgs);

         if (diagramDragEventArgs.Handled)
            return;

         List<BidirectionalConnector> bidirectionalConnectorsUnderShape = ClassShape.ClassShapeDragData?.GetBidirectionalConnectorsUnderShape(diagramDragEventArgs.MousePosition);

         //foreach (BidirectionalConnector connector in bidirectionalConnectorsUnderShape)
         //   Highlight(connector);

         bool isDroppingAssociationClass = bidirectionalConnectorsUnderShape?.Any() == true;

         if (isDroppingAssociationClass)
            diagramDragEventArgs.Effect = DragDropEffects.Link;
         else if (diagramDragEventArgs.Data.GetData("Sawczyn.EFDesigner.EFModel.ModelEnum") is ModelEnum
               || diagramDragEventArgs.Data.GetData("Sawczyn.EFDesigner.EFModel.ModelClass") is ModelClass
               || IsAcceptableDropItem(diagramDragEventArgs))
            diagramDragEventArgs.Effect = DragDropEffects.Copy;
         else
            diagramDragEventArgs.Effect = DragDropEffects.None;

         diagramDragEventArgs.Handled = true;
      }

      /// <summary>
      /// This method is called when a shape is inititially created, derived classes can
      /// override to perform shape instance initialization.  This method is always called within a transaction.
      /// </summary>
      public override void OnInitialize()
      {
         base.OnInitialize();
         ModelRoot modelRoot = Store.ModelRoot();

         // because we can hide elements, line routing looks odd when it thinks it's jumping over lines
         // that really aren't visible. Since replacing the routing algorithm is too hard (impossible?)
         // let's just stop it from showing jumps at all. A change to the highlighting on mouseover
         // makes it easier to see which lines are which in complex diagrams, so this doesn't hurt anything.
         RouteJumpType = VGPageLineJumpCode.NoJumps;

         ShowGrid = modelRoot?.ShowGrid ?? true;
         GridColor = modelRoot?.GridColor ?? Color.Gainsboro;
         SnapToGrid = modelRoot?.SnapToGrid ?? true;

         if (ModelDisplay.GetDiagramColors != null)
            SetThemeColors(ModelDisplay.GetDiagramColors());
      }

      /// <summary>Called by the control's OnMouseDown().</summary>
      /// <param name="e">A DiagramMouseEventArgs that contains event data.</param>
      public override void OnMouseDown(DiagramMouseEventArgs e)
      {
         MouseDownPosition = e.MousePosition;
         base.OnMouseDown(e);
      }

      /// <summary>Called by the control's OnMouseUp().</summary>
      /// <param name="e">A DiagramMouseEventArgs that contains event data.</param>
      public override void OnMouseUp(DiagramMouseEventArgs e)
      {
         IsDroppingExternal = false;
         ClassShape.ClassShapeDragData = null;
         base.OnMouseUp(e);
      }

      /// <summary>
      ///    Called during view fixup to ask the parent whether a shape should be created for the given child element.
      /// </summary>
      /// <remarks>
      ///    Always return true, since we assume there is only one diagram per model file for DSL scenarios.
      /// </remarks>
      protected override bool ShouldAddShapeForElement(ModelElement element)
      {
         ModelElement parent = null;

         // for attributes and enum values, the we should add the shape if its parent is on the diagram
         if (element is ModelAttribute modelAttribute)
            parent = modelAttribute.ModelClass;

         if (element is ModelEnumValue enumValue)
            parent = enumValue.Enum;

         bool result =
            ForceAddShape // we've made the decision somewhere else that this shape should be added 
         || IsDroppingExternal // we're dropping a file from Solution Explorer or File Explorer
         || base.ShouldAddShapeForElement(element) // the built-in rules say to do this
         || DisplayedElements.Contains(element) // the serialized diagram has this element present (other rules should prevent duplication)
         || ((parent != null) && DisplayedElements.Contains(parent)) // the element's parent is on this diagram
         || (element is ElementLink link && link.LinkedElements.All(linkedElement => DisplayedElements.Contains(linkedElement))); // adding a link and both of the linkk's end nodes are in this diagram

         return result;
      }

      /// <summary>
      /// Removes the highlight from the specified shape
      /// </summary>
      /// <param name="shape">The shape to remove the highlight from</param>
      public void Unhighlight(ShapeElement shape)
      {
         ShapeElement highlightShape = DetermineHighlightShape(shape);

         if (highlightShape != null)
            ActiveDiagramView.DiagramClientView.HighlightedShapes.Remove(new DiagramItem(highlightShape));
      }
   }
}