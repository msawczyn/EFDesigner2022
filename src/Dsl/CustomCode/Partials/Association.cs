using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Diagrams;
using Microsoft.VisualStudio.Modeling.Validation;

using Sawczyn.EFDesigner.EFModel.Annotations;
using Sawczyn.EFDesigner.EFModel.Extensions;

namespace Sawczyn.EFDesigner.EFModel
{
   [ValidationState(ValidationState.Enabled)]
   public partial class Association : IDisplaysWarning, IHasStore
   {
      internal string _targetBackingFieldName;

      /// <summary>
      ///    Gets the principal ModelClass of this association, if any
      /// </summary>
      public ModelClass Principal
      {
         get
         {
            return SourceRole == EndpointRole.Principal
                      ? Source
                      : TargetRole == EndpointRole.Principal
                         ? Target
                         : null;
         }
      }

      /// <summary>
      ///    Gets the dependent ModelClass of this association, if any
      /// </summary>
      public ModelClass Dependent
      {
         get
         {
            return SourceRole == EndpointRole.Dependent
                      ? Source
                      : TargetRole == EndpointRole.Dependent
                         ? Target
                         : null;
         }
      }

      internal string TargetBackingFieldNameDefault
      {
         get
         {
            return string.IsNullOrEmpty(TargetPropertyName)
                      ? string.Empty
                      : $"_{TargetPropertyName.Substring(0, 1).ToLowerInvariant()}{TargetPropertyName.Substring(1)}";
         }
      }

      internal bool AllCardinalitiesAreValid(out string errorMessage)
      {
         bool needsForeignKeyFixup = false;
         return AllCardinalitiesAreValid(out errorMessage, ref needsForeignKeyFixup);
      }

      internal bool AllCardinalitiesAreValid(out string errorMessage, ref bool needsForeignKeyFixup)
      {
         SetEndpointRoles();

         ModelRoot modelRoot = Source.ModelRoot;
         errorMessage = null;

         if (modelRoot.IsEFCore5Plus)
         {
            if (!modelRoot.IsEFCore7Plus && this is UnidirectionalAssociation && Is(Multiplicity.ZeroMany, Multiplicity.ZeroMany))
            {
               errorMessage = $"{GetDisplayText()}: many-to-many unidirectional associations are not yet supported in Entity Framework Core.";

               return false;
            }
         }
         else
         {
            if (this is UnidirectionalAssociation && Source.IsDependent() && !Target.IsDependent())
            {
               errorMessage = $"{GetDisplayText()}: dependent objects can't have associations to entities";

               return false;
            }
         }

         if (!Source.IsDependentType && !Target.IsDependentType)
         {
            if (Is(Multiplicity.One, Multiplicity.One) || Is(Multiplicity.ZeroOne, Multiplicity.ZeroOne))
            {
               if (SourceRole != EndpointRole.NotSet)
                  SourceRole = EndpointRole.NotSet;

               if (TargetRole != EndpointRole.NotSet)
                  TargetRole = EndpointRole.NotSet;
            }
            else
               SetEndpointRoles();
         }

         // cascade delete behavior could now be illegal. Reset to default
         SourceDeleteAction = DeleteAction.Default;
         TargetDeleteAction = DeleteAction.Default;

         if (Dependent == null)
         {
            FKPropertyName = null;
            needsForeignKeyFixup = true;
         }

         if ((modelRoot.EntityFrameworkVersion == EFVersion.EF6 || modelRoot.IsEFCore5Plus)
          && (SourceMultiplicity != Multiplicity.ZeroMany)
          && (TargetMultiplicity != Multiplicity.ZeroMany))
         {
            FKPropertyName = null;
            needsForeignKeyFixup = true;
         }

         if (Source.IsDependentType)
         {
            if (TargetMultiplicity == Multiplicity.ZeroMany)
            {
               errorMessage = $"{GetDisplayText()}: There can only be one owner in a dependent association";

               return false;
            }

            if (!modelRoot.IsEFCore5Plus)
            {
               if ((TargetMultiplicity != Multiplicity.One) || (SourceMultiplicity != Multiplicity.ZeroOne))
               {
                  errorMessage = $"{GetDisplayText()}: The association from {Source.Name} to {Target.Name} must be 1..0-1";

                  return false;
               }
            }
         }
         else if (Target.IsDependentType)
         {
            if (SourceMultiplicity == Multiplicity.ZeroMany)
            {
               errorMessage = $"{GetDisplayText()}: There can only be one owner in a dependent association";

               return false;
            }

            if (modelRoot.IsEFCore5Plus)
            {
               if (this is UnidirectionalAssociation && (TargetMultiplicity == Multiplicity.ZeroMany))
               {
                  errorMessage = $"{GetDisplayText()}: to-many associations to dependent objects must be bidirectional";

                  return false;
               }
            }
            else
            {
               if ((SourceMultiplicity != Multiplicity.One) || (TargetMultiplicity != Multiplicity.ZeroOne))
               {
                  errorMessage = $"{GetDisplayText()}: The association from {Target.Name} to {Source.Name} must be 1..0-1";

                  return false;
               }
            }
         }

         if (Source.Persistent ^ Target.Persistent)
         {
            if (Target.Persistent && !Source.Persistent && SourceRole != EndpointRole.Dependent)
            {
               // source is transient, target is persistent. Source must be dependent
               errorMessage = $"Unsupported association between {Source.Name} and {Target.Name}. The transient class must be the Dependent.";
               return false;
            }

            if (Source.Persistent && !Target.Persistent && TargetRole != EndpointRole.Dependent)
            {
               // source is persistent, target is transient. Target must be dependent
               errorMessage = $"Unsupported association between {Source.Name} and {Target.Name}. The transient class must be the Dependent.";
               return false;
            }
         }

         return true;
      }

      internal void FixupForeignKeys()
      {
         // for this to work, we need to know what's Principal and what's Dependent
         if (Principal == null || Dependent == null)
            return;

         // clear FK data from properties in the Principal class, if they exist
         foreach (ModelAttribute attribute in Principal.Attributes.Where(x => x.IsForeignKeyFor == Id))
            attribute.ClearFKMods(string.Empty);

         List<ModelAttribute> fkProperties = Dependent.Attributes
                                                 .Where(x => x.IsForeignKeyFor == Id)
                                                 .ToList();


         // EF6 can't have declared foreign keys for 1..1 / 0-1..1 / 1..0-1 / 0-1..0-1 relationships
         if (!string.IsNullOrEmpty(FKPropertyName)
          && (Source.ModelRoot.EntityFrameworkVersion == EFVersion.EF6)
          && (SourceMultiplicity != Multiplicity.ZeroMany)
          && (TargetMultiplicity != Multiplicity.ZeroMany))
            FKPropertyName = null;

         // if no FKs, remove FK properties in the Dependent class, if they exist
         if (string.IsNullOrEmpty(FKPropertyName))
         {
            List<ModelAttribute> unnecessaryProperties = fkProperties.Where(x => !x.IsIdentity).ToList();

            if (unnecessaryProperties.Any())
               WarningDisplay.Show($"{GetDisplayText()} doesn't specify defined foreign keys. Removing foreign key attribute(s) {string.Join(", ", unnecessaryProperties.Select(x => x.GetDisplayText()))}");

            foreach (ModelAttribute attribute in unnecessaryProperties)
            {
               attribute.ClearFKMods(string.Empty);
               attribute.Delete();
            }

            return;
         }

         // synchronize what's there to what should be there
         string[] currentForeignKeyPropertyNames = GetForeignKeyPropertyNames();

         (IEnumerable<string> add, IEnumerable<ModelAttribute> remove) = fkProperties.Synchronize(currentForeignKeyPropertyNames, (attribute, name) => attribute.Name == name);
         List<ModelAttribute> removeList = remove.ToList();
         List<string> addList = add.ToList();
         fkProperties = fkProperties.Except(removeList).ToList();

         // remove extras
         if (removeList.Any())
            WarningDisplay.Show($"{GetDisplayText()} has extra foreign keys. Removing unnecessary foreign key attribute(s) {string.Join(", ", removeList.Select(x => x.GetDisplayText()))}");

         for (int index = 0; index < removeList.Count; index++)
         {
            ModelAttribute attribute = removeList[index];
            attribute.ClearFKMods(string.Empty);
            attribute.Delete();
            removeList.RemoveAt(index--);
         }

         // reparent existing properties if needed
         Debug.WriteLine($"AssociationChangedRules.FixupForeignKeys: {GetDisplayText()} has {fkProperties.Count} FK properties");
         Debug.WriteLine($"AssociationChangedRules.FixupForeignKeys: Dependent class is {Dependent.Name}");
         Debug.WriteLine($"AssociationChangedRules.FixupForeignKeys: Principal class is {Principal.Name}");

         foreach (ModelAttribute existing in fkProperties.Where(x => x.ModelClass != Dependent))
         {
            Debug.WriteLine($"AssociationChangedRules.FixupForeignKeys: Reparenting {existing.Name} from {existing.ModelClass.Name} to {Dependent.Name}");

            existing.ClearFKMods();
            existing.ModelClass.MoveAttribute(existing, Dependent);
            existing.SetFKMods(this);
         }

         // create new properties if they don't already exist
         foreach (string propertyName in addList.Where(n => Dependent.Attributes.All(a => a.Name != n)))
            Dependent.Attributes.Add(new ModelAttribute(Store, new PropertyAssignment(ModelAttribute.NameDomainPropertyId, propertyName)));

         // make a pass through and fixup the types, summaries, etc. based on the principal's identity attributes
         ModelAttribute[] principalIdentityAttributes = Principal.AllIdentityAttributes.ToArray();
         string summaryBoilerplate = GetSummaryBoilerplate();

         Debug.WriteLine($"AssociationChangedRules.FixupForeignKeys: Principal has {Principal.AllIdentityAttributes.Count()} identity attributes");
         Debug.WriteLine($"AssociationChangedRules.FixupForeignKeys: Principal identity attributes are: {string.Join(", ", Principal.AllIdentityAttributes.Select(x => x.Name))}");
         Debug.WriteLine($"AssociationChangedRules.FixupForeignKeys: {GetDisplayText()} has {currentForeignKeyPropertyNames.Length} FK properties");
         Debug.WriteLine($"AssociationChangedRules.FixupForeignKeys: FK properties are: {string.Join(", ", currentForeignKeyPropertyNames)}");

         for (int index = 0; index < currentForeignKeyPropertyNames.Length; index++)
         {
            ModelAttribute fkProperty = Dependent.Attributes.First(x => x.Name == currentForeignKeyPropertyNames[index]);
            ModelAttribute idProperty = principalIdentityAttributes[index];

            bool required = Dependent == Source
                               ? TargetMultiplicity == Multiplicity.One
                               : SourceMultiplicity == Multiplicity.One;

            fkProperty.SetFKMods(this, summaryBoilerplate, required, idProperty.Type);
         }
      }

      internal bool SetEndpointRoles()
      {
         // Note that we're checking 'if (x != y) x = y' to ensure that unnecessary rules don't fire off

         switch (TargetMultiplicity)
         {
            case Multiplicity.ZeroMany:

               switch (SourceMultiplicity)
               {
                  case Multiplicity.ZeroMany:
                     if (SourceRole != EndpointRole.NotSet)
                        SourceRole = EndpointRole.NotSet;

                     if (TargetRole != EndpointRole.NotSet)
                        TargetRole = EndpointRole.NotSet;

                     return true;

                  case Multiplicity.One:
                     if (SourceRole != EndpointRole.Principal)
                        SourceRole = EndpointRole.Principal;

                     if (TargetRole != EndpointRole.Dependent)
                        TargetRole = EndpointRole.Dependent;

                     return true;

                  case Multiplicity.ZeroOne:
                     if (SourceRole != EndpointRole.Principal)
                        SourceRole = EndpointRole.Principal;

                     if (TargetRole != EndpointRole.Dependent)
                        TargetRole = EndpointRole.Dependent;

                     return true;
               }

               break;

            case Multiplicity.One:

               switch (SourceMultiplicity)
               {
                  case Multiplicity.ZeroMany:
                     if (SourceRole != EndpointRole.Dependent)
                        SourceRole = EndpointRole.Dependent;

                     if (TargetRole != EndpointRole.Principal)
                        TargetRole = EndpointRole.Principal;

                     return true;

                  case Multiplicity.One:

                     return false;

                  case Multiplicity.ZeroOne:
                     if (SourceRole != EndpointRole.Dependent)
                        SourceRole = EndpointRole.Dependent;

                     if (TargetRole != EndpointRole.Principal)
                        TargetRole = EndpointRole.Principal;

                     return true;
               }

               break;

            case Multiplicity.ZeroOne:

               switch (SourceMultiplicity)
               {
                  case Multiplicity.ZeroMany:
                     if (SourceRole != EndpointRole.Dependent)
                        SourceRole = EndpointRole.Dependent;

                     if (TargetRole != EndpointRole.Principal)
                        TargetRole = EndpointRole.Principal;

                     return true;

                  case Multiplicity.One:
                     if (SourceRole != EndpointRole.Principal)
                        SourceRole = EndpointRole.Principal;

                     if (TargetRole != EndpointRole.Dependent)
                        TargetRole = EndpointRole.Dependent;

                     return true;

                  case Multiplicity.ZeroOne:

                     return false;
               }

               break;
         }

         return false;
      }

      internal ModelClass GetAssociationClass()
      {
         return Source?.ModelRoot?.Classes.FirstOrDefault(x => x.IsAssociationClass && x.DescribedAssociationElementId == Id);
      }

      #region Validations

      [ValidationMethod(ValidationCategories.Save | ValidationCategories.Menu)]
      [UsedImplicitly]
      [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by validation")]
      private void FKPropertiesCannotBeStoreGeneratedIdentifiers(ValidationContext context)
      {
         foreach (ModelAttribute attribute in GetFKAutoIdentityErrors())
            context.LogError($"{GetDisplayText()}: FK property {attribute.Name} in {Dependent.FullName} is an auto-generated identity. Migration will fail.", "AEIdentityFK", this);
      }

      [ValidationMethod(ValidationCategories.Save | ValidationCategories.Menu)]
      [UsedImplicitly]
      [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by validation")]
      private void FKPropertiesInvalidWithoutDependentEnd(ValidationContext context)
      {
         if (string.IsNullOrWhiteSpace(FKPropertyName))
            return;

         if (Dependent == null)
            context.LogError($"{GetDisplayText()}: FK property set without association having a Dependent end.", "AEFKWithNoDependent", this);
      }

      [ValidationMethod(ValidationCategories.Save | ValidationCategories.Menu)]
      [UsedImplicitly]
      [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by validation")]
      private void FKPropertiesMatchIdentityProperties(ValidationContext context)
      {
         if (string.IsNullOrWhiteSpace(FKPropertyName) || (Principal == null))
            return;

         int identityCount = Principal.AllIdentityAttributes.Count();
         int fkCount = GetForeignKeyPropertyNames().Length;

         if (fkCount != identityCount)
         {
            context.LogError($"{GetDisplayText()}: Wrong number of FK properties. Should be {identityCount} to match identity count in {Principal.Name} - currently is {fkCount}.",
                             "AEFKWrongCount",
                             this);
         }
      }

      [ValidationMethod(ValidationCategories.Save | ValidationCategories.Menu)]
      [UsedImplicitly]
      [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by validation")]
      private void MustDetermineEndpointRoles(ValidationContext context)
      {
         if (Source?.ModelRoot == null)
            return;

         if ((Source != null)
          && (Target != null)
          && ((SourceRole == EndpointRole.NotSet) || (TargetRole == EndpointRole.NotSet))
          && (((SourceMultiplicity == Multiplicity.One) && (TargetMultiplicity == Multiplicity.One))
           || ((SourceMultiplicity == Multiplicity.ZeroOne) && (TargetMultiplicity == Multiplicity.ZeroOne))))
            context.LogError($"{GetDisplayText()}: Principal/dependent designations must be manually set for 1..1 and 0-1..0-1 associations.", "AEEndpointRoles", this);
      }

      [ValidationMethod(ValidationCategories.Open | ValidationCategories.Save | ValidationCategories.Menu)]
      [UsedImplicitly]
      [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by validation")]
      private void SummaryDescriptionIsEmpty(ValidationContext context)
      {
         if (Source?.ModelRoot == null)
            return;

         ModelRoot modelRoot = Store.ElementDirectory.FindElements<ModelRoot>().FirstOrDefault();

         if ((modelRoot?.WarnOnMissingDocumentation == true) && (Source != null) && string.IsNullOrWhiteSpace(TargetSummary))
         {
            context.LogWarning($"{Source.Name}.{TargetPropertyName}: Association end should be documented", "AWMissingSummary", this);
            hasWarning = true;
            RedrawItem();
         }
      }

      [ValidationMethod(ValidationCategories.Save | ValidationCategories.Menu)]
      [UsedImplicitly]
      [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by validation")]
      private void TPCEndpointsOnlyOnLeafNodes(ValidationContext context)
      {
         if (Source?.ModelRoot == null)
            return;

         if ((Source.InheritanceStrategy == CodeStrategy.TablePerConcreteType && Source.Subclasses.Any())
          || (Target.InheritanceStrategy == CodeStrategy.TablePerConcreteType && Target.Subclasses.Any()))
            context.LogError($"{GetDisplayText()}: Association endpoints can only be on most-derived classes in TPC inheritance strategy", "AEWrongEndpoints", this);
      }

      [ValidationMethod(ValidationCategories.Save | ValidationCategories.Menu)]
      [UsedImplicitly]
      [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by validation")]
      private void ValidateMultiplicity(ValidationContext context)
      {
         using (Transaction _ = Store.TransactionManager.BeginTransaction())
         {
            if (Persistent && !AllCardinalitiesAreValid(out string errorMessage))
               context.LogError(errorMessage, "AEUnsupportedMultiplicity", this);
         }
      }

      [ValidationMethod(ValidationCategories.Save | ValidationCategories.Menu)]
      [UsedImplicitly]
      [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by validation")]
      private void DependentsMustHaveIdentities(ValidationContext context)
      {
         using (Transaction _ = Store.TransactionManager.BeginTransaction())
         {
            if (Dependent != null && !Dependent.AllIdentityAttributes.Any())
               context.LogError($"{GetDisplayText()}: Dependent end must have at least one identity attribute", "AEDependentIdentity", this);
         }
      }

      [ValidationMethod(ValidationCategories.Save | ValidationCategories.Menu)]
      [UsedImplicitly]
      [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by validation")]
      private void JsonRequiresTPHInheritanceOnPrincipal(ValidationContext context)
      {
         using (Transaction _ = Store.TransactionManager.BeginTransaction())
         {
            if (Principal?.Persistent == true 
             && Dependent?.Persistent == false 
             && Principal.InheritanceStrategy != CodeStrategy.TablePerHierarchy)
               context.LogError($"{GetDisplayText()}: {Principal.Name} must be in a TPH inheritance strategy to support Json serialization", "AEJsonRequiresTPH", this);
         }
      }

      #endregion Validations

      /// <summary>
      ///    Short display text for this attribute
      /// </summary>
      public virtual string GetDisplayText()
      {
         string targetAutoIncluded = TargetAutoInclude
                                        ? " (AutoInclude)"
                                        : string.Empty;

         return $"{Source.Name}.{TargetPropertyName}{targetAutoIncluded} --> {Target.Name}";
      }

      internal IEnumerable<ModelAttribute> GetFKAutoIdentityErrors()
      {
         if (string.IsNullOrWhiteSpace(FKPropertyName) || (Dependent == null))
            return Array.Empty<ModelAttribute>();

         return FKPropertyName.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(name => Dependent.Attributes.FirstOrDefault(a => a.Name == name.Trim()))
                              .Where(a => (a != null) && a.IsIdentity && (a.IdentityType == IdentityType.AutoGenerated))
                              .ToList();
      }

      /// <summary>
      ///    Gets the individual foreign key property names defined in the FKPropertyName property
      /// </summary>
      public string[] GetForeignKeyPropertyNames()
      {
         return FKPropertyName?.Split(',').Select(n => n.Trim()).ToArray() ?? Array.Empty<string>();
      }

      private string GetNameValue()
      {
         return GetDisplayText();
      }

      /// <summary>
      ///    Gets a human-readable value for source multiplicity
      /// </summary>
      /// <returns></returns>
      public string GetSourceMultiplicityDisplayValue()
      {
         return MultiplicityDisplayValue(SourceMultiplicity);
      }

      internal string GetSummaryBoilerplate()
      {
         return $"Foreign key for {GetDisplayText()}";
      }

      /// <summary>
      ///    Returns the calculated name of the backing field for the Target end of this association
      /// </summary>
      protected string GetTargetBackingFieldNameValue()
      {
         return string.IsNullOrEmpty(_targetBackingFieldName)
                   ? TargetBackingFieldNameDefault
                   : _targetBackingFieldName;
      }

      /// <summary>
      ///    Gets a human-readable value for target multiplicity
      /// </summary>
      /// <returns></returns>
      public string GetTargetMultiplicityDisplayValue()
      {
         return MultiplicityDisplayValue(TargetMultiplicity);
      }

      private string GetTargetPropertyNameDisplayValue()
      {
         return (SourceRole == EndpointRole.Dependent) && !string.IsNullOrWhiteSpace(FKPropertyName) && Source.ModelRoot.ShowForeignKeyPropertyNames
                   ? $"{TargetPropertyName}\n[{string.Join(", ", GetForeignKeyPropertyNames().Select(n => $"{Source.Name}.{n.Trim()}"))}]"
                   : TargetPropertyName;
      }

      /// <summary>
      ///    Tests for multiplicities of the association, regardless of which end they're on (e.g., a-b or b-a)
      /// </summary>
      /// <param name="a">First multiplicity</param>
      /// <param name="b">Second multiplicity</param>
      /// <returns>True if the association contains the multplicities specified, false otherwise.</returns>
      public bool Is(Multiplicity a, Multiplicity b)
      {
         return ((SourceMultiplicity == a) && (TargetMultiplicity == b)) || ((SourceMultiplicity == b) && (TargetMultiplicity == a));
      }

      private static string MultiplicityDisplayValue(Multiplicity multiplicity)
      {
         switch (multiplicity)
         {
            case Multiplicity.One:
               return "1";

            //case Multiplicity.OneMany:
            //   return "1..*";
            case Multiplicity.ZeroMany:
               return "*";

            case Multiplicity.ZeroOne:
               return "0..1";
         }

         return "?";
      }

      /// <summary>
      ///    Calls the pre-reset method on the associated property value handler for each
      ///    tracking property of this model element.
      /// </summary>

      // ReSharper disable once UnusedMember.Global
      internal virtual void PreResetIsTrackingProperties()
      {
         IsCollectionClassTrackingPropertyHandler.Instance.PreResetValue(this);
         IsTargetImplementNotifyTrackingPropertyHandler.Instance.PreResetValue(this);
         IsTargetAutoPropertyTrackingPropertyHandler.Instance.PreResetValue(this);

         // same with other tracking properties as they get added
      }

      /// <summary>
      ///    Calls the reset method on the associated property value handler for each
      ///    tracking property of this model element.
      /// </summary>

      // ReSharper disable once UnusedMember.Global
      internal virtual void ResetIsTrackingProperties()
      {
         IsCollectionClassTrackingPropertyHandler.Instance.ResetValue(this);
         IsTargetImplementNotifyTrackingPropertyHandler.Instance.ResetValue(this);
         IsTargetAutoPropertyTrackingPropertyHandler.Instance.ResetValue(this);

         // same with other tracking properties as they get added
      }

      /// <summary>
      ///    Sets an override for the name of the backing field for the Target end of this association
      /// </summary>
      protected void SetTargetBackingFieldNameValue(string value)
      {
         _targetBackingFieldName = value;
      }

      internal sealed partial class SourceMultiplicityPropertyHandler
      {
         /// <summary>Called after property value has been changed.</summary>
         /// <param name="element">Element which owns the property.</param>
         /// <param name="oldValue">Old value of the property.</param>
         /// <param name="newValue">New value of the property.</param>
         protected override void OnValueChanged(Association element, Multiplicity oldValue, Multiplicity newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);

            if (!element.Store.InUndoRedoOrRollback)
               element.Store.DomainDataDirectory.GetDomainProperty(SourceMultiplicityDisplayDomainPropertyId).NotifyValueChange(element);
         }
      }

      internal sealed partial class TargetMultiplicityPropertyHandler
      {
         /// <summary>Called after property value has been changed.</summary>
         /// <param name="element">Element which owns the property.</param>
         /// <param name="oldValue">Old value of the property.</param>
         /// <param name="newValue">New value of the property.</param>
         protected override void OnValueChanged(Association element, Multiplicity oldValue, Multiplicity newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);

            if (!element.Store.InUndoRedoOrRollback)
               element.Store.DomainDataDirectory.GetDomainProperty(TargetMultiplicityDisplayDomainPropertyId).NotifyValueChange(element);
         }
      }

      #region Value changed handlers

      //partial class FKPropertyNamePropertyHandler
      //{
      //   protected override void OnValueChanged(Association element, string oldValue, string newValue)
      //   {
      //      base.OnValueChanged(element, oldValue, newValue);

      //      ModelRoot modelRoot = element.Store.ModelRoot();

      //      if (!element.Store.InUndoRedoOrRollback)
      //      {
      //         if (modelRoot.EntityFrameworkVersion == EFVersion.EF6 && element.SourceMultiplicity != Multiplicity.ZeroMany && element.TargetMultiplicity != Multiplicity.ZeroMany && newValue != null)
      //         {
      //            element.FKPropertyName = null;
      //            AssociationChangedRules.FixupForeignKeys(element);
      //         }
      //      }
      //   }

      //}

      #endregion

      #region Warning display

      // set as methods to avoid issues around serialization

      /// <summary>
      ///    If true, this association has a warning and might show a glyph in the diagram
      /// </summary>
      protected bool hasWarning;

      /// <inheritdoc />
      public bool GetHasWarningValue()
      {
         return hasWarning;
      }

      /// <inheritdoc />
      public void ResetWarning()
      {
         hasWarning = false;
      }

      /// <inheritdoc />
      public void RedrawItem()
      {
         ModelElement[] modelElements = { this, Source, Target };

         // redraw on every diagram
         foreach (ShapeElement shapeElement in
                  modelElements.SelectMany(modelElement => PresentationViewsSubject.GetPresentation(modelElement)
                                                                                   .OfType<ShapeElement>()
                                                                                   .Distinct()))
            shapeElement.Invalidate();
      }

      #endregion

      #region TargetImplementNotify tracking property

      /// <summary>Storage for the TargetImplementNotify property.</summary>
      private bool targetImplementNotifyStorage;

      /// <summary>Gets the storage for the TargetImplementNotify property.</summary>
      /// <returns>The TargetImplementNotify value.</returns>
      public bool GetTargetImplementNotifyValue()
      {
         if (!this.IsLoading() && IsTargetImplementNotifyTracking)
         {
            try
            {
               return Target?.ImplementNotify ?? false;
            }
            catch (NullReferenceException)
            {
               return false;
            }
            catch (Exception e)
            {
               if (CriticalException.IsCriticalException(e))
                  throw;

               return false;
            }
         }

         return targetImplementNotifyStorage;
      }

      /// <summary>Sets the storage for the TargetImplementNotify property.</summary>
      /// <param name="value">The TargetImplementNotify value.</param>
      public void SetTargetImplementNotifyValue(bool value)
      {
         targetImplementNotifyStorage = value;

         if (!Store.InUndoRedoOrRollback && !this.IsLoading())

            // ReSharper disable once ArrangeRedundantParentheses
            IsTargetImplementNotifyTracking = (targetImplementNotifyStorage == Target.ImplementNotify);
      }

      internal sealed partial class IsTargetImplementNotifyTrackingPropertyHandler
      {
         /// <summary>
         ///    Called after the IsTargetImplementNotifyTracking property changes.
         /// </summary>
         /// <param name="element">The model element that has the property that changed. </param>
         /// <param name="oldValue">The previous value of the property. </param>
         /// <param name="newValue">The new value of the property. </param>
         protected override void OnValueChanged(Association element, bool oldValue, bool newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);

            if (!element.Store.InUndoRedoOrRollback && newValue)
            {
               DomainPropertyInfo propInfo = element.Store.DomainDataDirectory.GetDomainProperty(TargetImplementNotifyDomainPropertyId);
               propInfo.NotifyValueChange(element);
            }
         }

         /// <summary>
         ///    Method to set IsTargetImplementNotifyTracking to false so that this instance of this tracking property is not
         ///    storage-based.
         /// </summary>
         /// <param name="element">
         ///    The element on which to reset the property value.
         /// </param>
         internal void PreResetValue(Association element)
         {
            // Force the IsTargetImplementNotifyTracking property to false so that the value  
            // of the TargetImplementNotify property is retrieved from storage.  
            element.isTargetImplementNotifyTrackingPropertyStorage = false;
         }

         /// <summary>Performs the reset operation for the IsTargetImplementNotifyTracking property for a model element.</summary>
         /// <param name="element">The model element that has the property to reset.</param>
         internal void ResetValue(Association element)
         {
            object calculatedValue = null;

            try
            {
               calculatedValue = element.Target?.ImplementNotify;
            }
            catch (NullReferenceException) { }
            catch (Exception e)
            {
               if (CriticalException.IsCriticalException(e))
                  throw;
            }

            if ((calculatedValue != null) && (element.TargetImplementNotify == (bool)calculatedValue))
               element.isTargetImplementNotifyTrackingPropertyStorage = true;
         }
      }

      #endregion

      #region CollectionClass tracking property

      private string collectionClassStorage;

      private string GetCollectionClassValue()
      {
         if (!this.IsLoading() && IsCollectionClassTracking)
         {
            try
            {
               return Source?.ModelRoot?.DefaultCollectionClass;
            }
            catch (NullReferenceException)
            {
               return default;
            }
            catch (Exception e)
            {
               if (CriticalException.IsCriticalException(e))
                  throw;

               return default;
            }
         }

         return collectionClassStorage;
      }

      private void SetCollectionClassValue(string value)
      {
         collectionClassStorage = value;

         if (!Store.InUndoRedoOrRollback && !this.IsLoading())
            IsCollectionClassTracking = value == Source.ModelRoot.DefaultCollectionClass;
      }

      internal sealed partial class IsCollectionClassTrackingPropertyHandler
      {
         /// <summary>
         ///    Called after the IsCollectionClassTracking property changes.
         /// </summary>
         /// <param name="element">The model element that has the property that changed. </param>
         /// <param name="oldValue">The previous value of the property. </param>
         /// <param name="newValue">The new value of the property. </param>
         protected override void OnValueChanged(Association element, bool oldValue, bool newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);

            if (!element.Store.InUndoRedoOrRollback && newValue)
            {
               DomainPropertyInfo propInfo = element.Store.DomainDataDirectory.GetDomainProperty(CollectionClassDomainPropertyId);
               propInfo.NotifyValueChange(element);
            }
         }

         /// <summary>
         ///    Method to set IsCollectionClassTracking to false so that this instance of this tracking property is not
         ///    storage-based.
         /// </summary>
         /// <param name="element">
         ///    The element on which to reset the property
         ///    value.
         /// </param>
         internal void PreResetValue(Association element)
         {
            // Force the IsCollectionClassTracking property to false so that the value  
            // of the CollectionClass property is retrieved from storage.  
            element.isCollectionClassTrackingPropertyStorage = false;
         }

         /// <summary>Performs the reset operation for the IsCollectionClassTracking property for a model element.</summary>
         /// <param name="element">The model element that has the property to reset.</param>
         internal void ResetValue(Association element)
         {
            object calculatedValue = null;

            try
            {
               ModelRoot modelRoot = element.Store.ModelRoot();
               calculatedValue = modelRoot.DefaultCollectionClass;
            }
            catch (NullReferenceException) { }
            catch (Exception e)
            {
               if (CriticalException.IsCriticalException(e))
                  throw;
            }

            if ((calculatedValue != null) && (element.CollectionClass == (string)calculatedValue))
               element.isCollectionClassTrackingPropertyStorage = true;
         }
      }

      #endregion CollectionClass tracking property

      #region TargetAutoProperty tracking property

      private bool targetAutoPropertyStorage;

      private bool GetTargetAutoPropertyValue()
      {
         if (!this.IsLoading() && IsTargetAutoPropertyTracking)
         {
            try
            {
               return Source?.AutoPropertyDefault ?? true;
            }
            catch (NullReferenceException)
            {
               return default;
            }
            catch (Exception e)
            {
               if (CriticalException.IsCriticalException(e))
                  throw;

               return default;
            }
         }

         return targetAutoPropertyStorage;
      }

      private void SetTargetAutoPropertyValue(bool value)
      {
         targetAutoPropertyStorage = value;

         if (!Store.InUndoRedoOrRollback && !this.IsLoading())
            IsTargetAutoPropertyTracking = value == Source.AutoPropertyDefault;
      }

      internal sealed partial class IsTargetAutoPropertyTrackingPropertyHandler
      {
         /// <summary>
         ///    Called after the IsTargetAutoPropertyTracking property changes.
         /// </summary>
         /// <param name="element">The model element that has the property that changed. </param>
         /// <param name="oldValue">The previous value of the property. </param>
         /// <param name="newValue">The new value of the property. </param>
         protected override void OnValueChanged(Association element, bool oldValue, bool newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);

            if (!element.Store.InUndoRedoOrRollback && newValue)
            {
               DomainPropertyInfo propInfo = element.Store.DomainDataDirectory.GetDomainProperty(TargetAutoPropertyDomainPropertyId);
               propInfo.NotifyValueChange(element);
            }
         }

         /// <summary>
         ///    Method to set IsTargetAutoPropertyTracking to false so that this instance of this tracking property is not
         ///    storage-based.
         /// </summary>
         /// <param name="element">
         ///    The element on which to reset the property
         ///    value.
         /// </param>
         internal void PreResetValue(Association element)
         {
            // Force the IsTargetAutoPropertyTracking property to false so that the value  
            // of the TargetAutoProperty property is retrieved from storage.  
            element.isTargetAutoPropertyTrackingPropertyStorage = false;
         }

         /// <summary>Performs the reset operation for the IsTargetAutoPropertyTracking property for a model element.</summary>
         /// <param name="element">The model element that has the property to reset.</param>
         internal void ResetValue(Association element)
         {
            object calculatedValue = null;

            try
            {
               calculatedValue = element.Source?.AutoPropertyDefault;
            }
            catch (NullReferenceException) { }
            catch (Exception e)
            {
               if (CriticalException.IsCriticalException(e))
                  throw;
            }

            if ((calculatedValue != null) && (element.TargetAutoProperty == (bool)calculatedValue))
               element.isTargetAutoPropertyTrackingPropertyStorage = true;
         }
      }

      #endregion TargetAutoProperty tracking property
   }
}