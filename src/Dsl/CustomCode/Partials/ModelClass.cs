using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Immutability;
using Microsoft.VisualStudio.Modeling.Validation;

using Sawczyn.EFDesigner.EFModel.Annotations;
using Sawczyn.EFDesigner.EFModel.Extensions;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local

namespace Sawczyn.EFDesigner.EFModel
{
   [ValidationState(ValidationState.Enabled)]
   public partial class ModelClass : IModelElementWithCompartments, IDisplaysWarning, IHasStore
   {
      /// <summary>
      ///    The default namespace for this entity, based on its kind
      /// </summary>
      [Browsable(false)]
      public string DefaultNamespace
      {
         get
         {
            if (IsDependentType && !string.IsNullOrWhiteSpace(ModelRoot?.StructNamespace))
               return ModelRoot.StructNamespace;

            if (!IsDependentType && !string.IsNullOrWhiteSpace(ModelRoot?.EntityNamespace))
               return ModelRoot.EntityNamespace;

            return ModelRoot?.Namespace;
         }
      }

      /// <summary>
      ///    Namespace for generated code. Takes overrides into account.
      /// </summary>
      [Browsable(false)]
      public string EffectiveNamespace
      {
         get
         {
            return namespaceStorage ?? DefaultNamespace;
         }
      }

      /// <summary>
      ///    Where in the project code would normally be generated, based on the entity type
      /// </summary>
      [Browsable(false)]
      public string DefaultOutputDirectory
      {
         get
         {
            if ((IsDependentType || !Persistent) && !string.IsNullOrWhiteSpace(ModelRoot?.StructOutputDirectory))
               return ModelRoot.StructOutputDirectory;

            if ((!IsDependentType && Persistent) && !string.IsNullOrWhiteSpace(ModelRoot?.EntityOutputDirectory))
               return ModelRoot.EntityOutputDirectory;

            return ModelRoot?.ContextOutputDirectory;
         }
      }

      /// <summary>
      ///    Output directory for generated code. Takes overrides into account.
      /// </summary>
      [Browsable(false)]
      public string EffectiveOutputDirectory
      {
         get
         {
            return outputDirectoryStorage ?? DefaultOutputDirectory;
         }
      }

      /// <summary>
      ///    All custom interfaces in the class, including those inherited from base classes
      /// </summary>
      public IEnumerable<string> AllCustomInterfaces
      {
         get
         {
            List<string> interfaces = new List<string>();
            ModelClass modelClass = this;

            while (modelClass != null)
            {
               interfaces.AddRange(modelClass.CustomInterfaces.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries));
               modelClass = modelClass.Superclass;
            }

            return interfaces.Distinct();
         }
      }

      /// <summary>
      ///    All attributes in the class, including those inherited from base classes
      /// </summary>
      public IEnumerable<ModelAttribute> AllAttributes
      {
         get
         {
            List<ModelAttribute> result = Attributes.ToList();

            if (Superclass != null)
               result.AddRange(Superclass.AllAttributes);

            return result;
         }
      }

      /// <summary>
      ///    Collection of all ModelClass classes making up the inheritance for this ModelClass
      /// </summary>
      public List<ModelClass> AllSuperclasses
      {
         get
         {
            List<ModelClass> result = new List<ModelClass>();
            ModelClass s = Superclass;

            while (s != null)
            {
               result.Add(s);
               s = s.Superclass;
            }

            return result;
         }
      }

      /// <summary>
      ///    Collection of all ModelClass classes where this ModelClass is in its inheritance chain
      /// </summary>
      public List<ModelClass> AllSubclasses
      {
         get
         {
            List<ModelClass> result = Store.GetAll<ModelClass>().Where(x => x.Superclass == this).ToList();

            for (int i = 0; i < result.Count; i++)
               result.AddRange(Store.GetAll<ModelClass>().Where(x => x.Superclass == result[i]));

            return result;
         }
      }

      /// <summary>
      ///    Names of all properties in the class, taking into account inheritance
      /// </summary>
      public IEnumerable<string> AllPropertyNames
      {
         get
         {
            List<string> result = AllAttributes.Select(a => a.Name).ToList();

            result.AddRange(AllNavigationProperties().Where(np => !string.IsNullOrEmpty(np.PropertyName)).Select(np => np.PropertyName));

            return result;
         }
      }

      /// <summary>
      ///    All required attributes defined in this class
      /// </summary>
      public IEnumerable<ModelAttribute> RequiredAttributes
      {
         get
         {
            return Attributes.Where(x => x.Required).ToList();
         }
      }

      /// <summary>
      ///    All required attributes in the inheritance chain
      /// </summary>
      public IEnumerable<ModelAttribute> AllRequiredAttributes
      {
         get
         {
            return AllAttributes.Where(x => x.Required).ToList();
         }
      }

      /// <summary>
      ///    All identity attributes defined in this class
      /// </summary>
      public IEnumerable<ModelAttribute> IdentityAttributes
      {
         get
         {
            return Attributes.Where(x => x.IsIdentity).ToList();
         }
      }

      /// <summary>
      ///    All identity attributes in the inheritance chain
      /// </summary>
      public IEnumerable<ModelAttribute> AllIdentityAttributes
      {
         get
         {
            return AllAttributes.Where(x => x.IsIdentity).ToList();
         }
      }

      /// <summary>
      ///    Names of identity attributes defined in this class
      /// </summary>
      public IEnumerable<string> IdentityAttributeNames
      {
         get
         {
            return IdentityAttributes.Select(x => x.Name).ToList();
         }
      }

      /// <summary>
      ///    Names of all identity attributes in the inheritance chain
      /// </summary>
      public IEnumerable<string> AllIdentityAttributeNames
      {
         get
         {
            return AllIdentityAttributes.Select(x => x.Name).ToList();
         }
      }

      /// <summary>
      ///    Class name with namespace
      /// </summary>
      public string FullName
      {
         get
         {
            if (IsPropertyBag)
               return "System.Collections.Generic.Dictionary<string, object>";

            return string.IsNullOrWhiteSpace(EffectiveNamespace)
                      ? $"global::{Name}"
                      : $"global::{EffectiveNamespace}.{Name}";
         }
      }

      /// <summary>
      ///    Concurrency type, taking into account the model's default concurrency and any override defined in this class
      /// </summary>
      public ConcurrencyOverride EffectiveConcurrency
      {
         get
         {
            if (Concurrency != ConcurrencyOverride.Default)
               return Concurrency;

            return ModelRoot?.ConcurrencyDefault == EFModel.Concurrency.None
                      ? ConcurrencyOverride.None
                      : ConcurrencyOverride.Optimistic;
         }
      }

      /// <summary>
      ///    All navigation properties including those in superclasses
      /// </summary>
      /// <param name="except">Associations to remove from the result</param>
      /// <returns>All navigation properties including those in superclasses, except those listed in the parameter</returns>
      public IEnumerable<NavigationProperty> AllNavigationProperties(params Association[] except)
      {
         List<NavigationProperty> result = LocalNavigationProperties(except).ToList();

         if (Superclass != null)
            result.AddRange(Superclass.AllNavigationProperties(except));

         return result;
      }

      /// <summary>
      ///    All the required navigation (1.. cardinality) properties in both this and base classes.
      /// </summary>
      /// <param name="except">Associations to remove from the result.</param>
      /// <returns>All required associations found, except for those in the [except] parameter</returns>
      public IEnumerable<NavigationProperty> AllRequiredNavigationProperties(params Association[] except)
      {
         return AllNavigationProperties(except).Where(x => x.Required).ToList();
      }

      internal bool CanBecomeAssociationClass()
      {
         return ModelRoot.IsEFCore5Plus
             && !AllNavigationProperties().Any()
             && string.IsNullOrEmpty(BaseClass)
             && !IsAssociationClass
             && !IsAbstract
             && !IsDatabaseView
             && !IsDependentType
             && !IsPropertyBag
             && !IsQueryType
             && (Superclass == null)
             && !Subclasses.Any()
             && string.IsNullOrEmpty(ViewName);
      }

      internal void ConvertToAssociationClass(BidirectionalAssociation bidirectionalAssociation)
      {
         if (ModelRoot == null)
            return;

         using (Transaction tx = Store.TransactionManager.BeginTransaction("Set association class"))
         {
            // add the new associations 

            // ReSharper disable once UnusedVariable
            BidirectionalAssociation element1 =
               new BidirectionalAssociation(Store,
                                            new[]
                                            {
                                               new RoleAssignment(Association.SourceDomainRoleId, bidirectionalAssociation.Source), 
                                               new RoleAssignment(Association.TargetDomainRoleId, this)
                                            },
                                            new[]
                                            {
                                               //  new PropertyAssignment(Association.TargetPropertyNameDomainPropertyId, $"{bidirectionalAssociation.TargetPropertyName}_{Name}")
                                               //, new PropertyAssignment(BidirectionalAssociation.SourcePropertyNameDomainPropertyId, $"{bidirectionalAssociation.SourcePropertyName}")
                                               new PropertyAssignment(Association.TargetDisplayTextDomainPropertyId, $"Association object for {bidirectionalAssociation.TargetPropertyName}"),
                                               new PropertyAssignment(BidirectionalAssociation.SourceDisplayTextDomainPropertyId,
                                                                      $"Association object for {bidirectionalAssociation.SourcePropertyName}"),
                                               new PropertyAssignment(Association.TargetSummaryDomainPropertyId, $"Association class for {bidirectionalAssociation.TargetPropertyName}"),
                                               new PropertyAssignment(BidirectionalAssociation.SourceSummaryDomainPropertyId, $"Association class for {bidirectionalAssociation.SourcePropertyName}"),
                                               new PropertyAssignment(Association.SourceMultiplicityDomainPropertyId, Multiplicity.One),
                                               new PropertyAssignment(Association.TargetMultiplicityDomainPropertyId, Multiplicity.ZeroMany),
                                               new PropertyAssignment(Association.FKPropertyNameDomainPropertyId, $"{bidirectionalAssociation.SourcePropertyName}Id")
                                            });

            // ReSharper disable once UnusedVariable
            BidirectionalAssociation element2 =
               new BidirectionalAssociation(Store,
                                            new[]
                                            {
                                               new RoleAssignment(Association.SourceDomainRoleId, bidirectionalAssociation.Target), 
                                               new RoleAssignment(Association.TargetDomainRoleId, this)
                                            },
                                            new[]
                                            {
                                               //  new PropertyAssignment(Association.TargetPropertyNameDomainPropertyId, $"{bidirectionalAssociation.SourcePropertyName}_{Name}")
                                               //, new PropertyAssignment(BidirectionalAssociation.SourcePropertyNameDomainPropertyId, $"{bidirectionalAssociation.TargetPropertyName}")
                                               new PropertyAssignment(Association.TargetDisplayTextDomainPropertyId, $"Association object for {bidirectionalAssociation.SourcePropertyName}"),
                                               new PropertyAssignment(BidirectionalAssociation.SourceDisplayTextDomainPropertyId,
                                                                      $"Association object for {bidirectionalAssociation.TargetPropertyName}"),
                                               new PropertyAssignment(Association.TargetSummaryDomainPropertyId, $"Association class for {bidirectionalAssociation.SourcePropertyName}"),
                                               new PropertyAssignment(BidirectionalAssociation.SourceSummaryDomainPropertyId, $"Association class for {bidirectionalAssociation.TargetPropertyName}"),
                                               new PropertyAssignment(Association.SourceMultiplicityDomainPropertyId, Multiplicity.One),
                                               new PropertyAssignment(Association.TargetMultiplicityDomainPropertyId, Multiplicity.ZeroMany),
                                               new PropertyAssignment(Association.FKPropertyNameDomainPropertyId, $"{bidirectionalAssociation.TargetPropertyName}Id")
                                            });

            // set some properties in the association class

            DescribedAssociationElementId = bidirectionalAssociation.Id;
            IsAssociationClass = true;

            // get rid of any identity attributes
            Attributes.Where(a => a.IsIdentity).ToList().ForEach(attribute => attribute.IsIdentity = false);

            // add the new FK properties

            AssociationChangedRules.FixupForeignKeys(element1);
            AssociationChangedRules.FixupForeignKeys(element2);

            // clean them up 

            ModelAttribute sourcePropertyIdAttribute = FindAttributeNamed($"{bidirectionalAssociation.SourcePropertyName}Id");
            sourcePropertyIdAttribute.IsIdentity = true;
            sourcePropertyIdAttribute.IdentityType = IdentityType.Manual;
            sourcePropertyIdAttribute.IndexedUnique = false;
            sourcePropertyIdAttribute.SetLocks(Locks.Delete);

            ModelAttribute targetPropertyIdAttribute = FindAttributeNamed($"{bidirectionalAssociation.TargetPropertyName}Id");
            targetPropertyIdAttribute.IsIdentity = true;
            targetPropertyIdAttribute.IdentityType = IdentityType.Manual;
            targetPropertyIdAttribute.IndexedUnique = false;
            targetPropertyIdAttribute.SetLocks(Locks.Delete);

            bidirectionalAssociation.JoinTableName = TableName;

            // and save it all
            tx.Commit();
         }
      }

      /// <summary>
      ///    Finds the association named by the value specified in the parameter
      /// </summary>
      /// <param name="identifier">Association property name to find.</param>
      /// <returns>The object representing the association, if found</returns>
      public NavigationProperty FindAssociationNamed(string identifier)
      {
         return AllNavigationProperties().FirstOrDefault(x => x.PropertyName == identifier);
      }

      /// <summary>
      ///    Finds the attribute named by the value specified in the parameter
      /// </summary>
      /// <param name="identifier">Attribute name to find.</param>
      /// <returns>The object representing the attribute, if found</returns>
      public ModelAttribute FindAttributeNamed(string identifier)
      {
         return AllAttributes.FirstOrDefault(x => x.Name == identifier);
      }

      /// <summary>
      ///    Gets the name of the superclass, if any.
      /// </summary>
      /// <returns></returns>
      private string GetBaseClassValue()
      {
         return Superclass?.Name;
      }

      /// <summary>
      ///    Name of the DbSet for this class unless overridden
      /// </summary>
      /// <param name="shouldPluralize">If true, the DbSet should be a plural form of the class name</param>
      /// <returns></returns>
      public string GetDefaultDbSetName(bool shouldPluralize)
      {
         return (ModelRoot.PluralizationService?.IsSingular(Name) == true) && shouldPluralize
                   ? ModelRoot.PluralizationService.Pluralize(Name)
                   : Name;
      }

      /// <summary>
      ///    Name of the table for this class unless overridden
      /// </summary>
      /// <param name="shouldPluralize">If true, the table should be a plural form of the class name</param>
      /// <returns></returns>
      public string GetDefaultTableName(bool shouldPluralize)
      {
         return (ModelRoot.PluralizationService?.IsSingular(Name) == true) && shouldPluralize
                   ? ModelRoot.PluralizationService.Pluralize(Name)
                   : Name;
      }

      /// <summary>
      ///    Human readable text for displaying this object
      /// </summary>
      /// <returns></returns>
      public string GetDisplayText()
      {
         return Name;
      }

      /// <summary>
      ///    Determines whether the generated code will have an association property with the name specified in the parameter
      /// </summary>
      /// <param name="identifier">Property name to find.</param>
      /// <returns>
      ///    <c>true</c> if the class will have this property; otherwise, <c>false</c>.
      /// </returns>
      public bool HasAssociationNamed(string identifier)
      {
         return FindAssociationNamed(identifier) != null;
      }

      /// <summary>
      ///    Determines whether the generated code will have a scalar property with the name specified in the parameter
      /// </summary>
      /// <param name="identifier">Property name to find.</param>
      /// <returns>
      ///    <c>true</c> if the class will have this property; otherwise, <c>false</c>.
      /// </returns>
      public bool HasAttributeNamed(string identifier)
      {
         return FindAttributeNamed(identifier) != null;
      }

      /// <summary>
      ///    Determines whether the generated code will have any property with the name specified in the parameter
      /// </summary>
      /// <param name="identifier">Property name to find.</param>
      /// <returns>
      ///    <c>true</c> if the class will have this property; otherwise, <c>false</c>.
      /// </returns>
      public bool HasPropertyNamed(string identifier)
      {
         return HasAssociationNamed(identifier) || HasAttributeNamed(identifier);
      }

      /// <summary>
      ///    True if this is a dependent (aggregated) entity type, false otherwise
      /// </summary>
      [Browsable(false)]
      public bool IsDependent()
      {
         return IsDependentType;
      }

      /// <summary>
      ///    True if this is a normal entity type (not aggregated and not keyless), false otherwise
      /// </summary>
      [Browsable(false)]
      public bool IsEntity()
      {
         return !IsDependentType && !IsKeylessType();
      }

      /// <summary>
      ///    True if this is a keyless entity type (backed by a query or a view), false otherwise
      /// </summary>
      [Browsable(false)]
      public bool IsKeyless()
      {
         return IsKeylessType();
      }

      internal bool IsKeylessType()
      {
         return IsQueryType || IsDatabaseView || !AllIdentityAttributes.Any();
      }

      /// <summary>
      ///    All navigation properties defined in this class
      /// </summary>
      /// <param name="except">Associations to remove from the result</param>
      /// <returns>All navigation properties defined in this class, except those listed in the parameter</returns>
      public IEnumerable<NavigationProperty> LocalNavigationProperties(params Association[] except)
      {
         List<NavigationProperty> sourceProperties = LocalNavigationsFromThisAsSource(except);

         List<NavigationProperty> targetProperties = LocalNavigationsFromThisAsTarget(except);

         targetProperties.AddRange(Association.GetLinksToSources(this)
                                              .Except(except)
                                              .OfType<UnidirectionalAssociation>()
                                              .Select(NavigationProperty.LinkToSource));

         int suffix = 0;

         foreach (NavigationProperty navigationProperty in targetProperties.Where(x => x.PropertyName == null))
         {
            navigationProperty.PropertyName = $"_{navigationProperty.ClassType.Name.ToLower()}{suffix++}";
            navigationProperty.ConstructorParameterOnly = true;
         }

         return sourceProperties.Concat(targetProperties);
      }

      /// <summary>
      /// Returns a list of navigation properties where the current object is the source of the relationship.
      /// </summary>
      /// <param name="except">An array of associations to exclude from the result.</param>
      /// <returns>A list of navigation properties.</returns>
      public List<NavigationProperty> LocalNavigationsFromThisAsSource(Association[] except)
      {
         return Association.GetLinksToTargets(this)
                           .Except(except)
                           .Select(NavigationProperty.LinkToTarget)
                           .ToList();
      }

      /// <summary>
      /// Returns a list of NavigationProperty objects representing associations 
      /// where the current object is the target and ignoring any specified in the 'except' array.
      /// </summary>
      /// <param name="except">Array of Association objects to ignore</param>
      /// <returns>List of NavigationProperty objects</returns>
      public List<NavigationProperty> LocalNavigationsFromThisAsTarget(Association[] except)
      {
         return Association.GetLinksToSources(this)
                           .Except(except)
                           .OfType<BidirectionalAssociation>()
                           .Select(NavigationProperty.LinkToSource)
                           .ToList();
      }

      internal void MoveAttribute(ModelAttribute attribute, ModelClass destination)
      {
         MergeDisconnect(attribute);
         destination.MergeRelate(attribute, null);
      }

      /// <summary>
      ///    Calls the pre-reset method on the associated property value handler for each
      ///    tracking property of this model element.
      /// </summary>
      public virtual void PreResetIsTrackingProperties()
      {
         IsDatabaseSchemaTrackingPropertyHandler.Instance.PreResetValue(this);
         IsNamespaceTrackingPropertyHandler.Instance.PreResetValue(this);
         IsOutputDirectoryTrackingPropertyHandler.Instance.PreResetValue(this);

         // same with other tracking properties as they get added
      }

      /// <summary>
      ///    required navigation (1.. cardinality) properties in this class
      /// </summary>
      /// <param name="except">Associations to remove from the result.</param>
      /// <returns>All required associations found, except for those in the [except] parameter</returns>
      public IEnumerable<NavigationProperty> RequiredNavigationProperties(params Association[] except)
      {
         return LocalNavigationProperties(except).Where(x => x.Required).ToList();
      }

      /// <summary>
      ///    Calls the reset method on the associated property value handler for each
      ///    tracking property of this model element.
      /// </summary>
      internal virtual void ResetIsTrackingProperties()
      {
         IsDatabaseSchemaTrackingPropertyHandler.Instance.ResetValue(this);
         IsNamespaceTrackingPropertyHandler.Instance.ResetValue(this);
         IsOutputDirectoryTrackingPropertyHandler.Instance.ResetValue(this);

         // same with other tracking properties as they get added
      }

      /// <summary>
      ///    Sets the superclass to the class with the supplied name, if it exists. Sets to null if can't be found.
      /// </summary>
      /// <param name="newValue">Simple name (not FQN) of class to use as superclass.</param>
      private void SetBaseClassValue(string newValue)
      {
         ModelClass baseClass = Store.ElementDirectory.FindElements<ModelClass>().FirstOrDefault(x => x.Name == newValue);
         Superclass = baseClass;
      }

#region Warning display

      // set as methods to avoid issues around serialization

      private bool hasWarning;

      /// <summary>
      ///    Determines if this class has warnings being displayed.
      /// </summary>
      /// <returns>True if this class has warnings visible, false otherwise</returns>
      public bool GetHasWarningValue()
      {
         return hasWarning;
      }

      /// <summary>
      ///    Clears visible warnings.
      /// </summary>
      public void ResetWarning()
      {
         hasWarning = false;
      }

      /// <summary>
      ///    Redraws this class.
      /// </summary>
      public void RedrawItem()
      {
         this.Redraw();
      }

      /// <summary>
      ///    If true, diagram should show an interface lollipop on the class if it has a custom interface
      /// </summary>
      protected bool GetShouldShowInterfaceGlyphValue()
      {
         return ModelRoot.ShowInterfaceIndicators && !string.IsNullOrEmpty(CustomInterfaces);
      }

      /// <summary>
      /// Gets the text of the tooptip that will be shown when hovering over the explorer node for this class
      /// </summary>
      /// <returns>Tooltip text</returns>
      [SuppressMessage( "ReSharper", "ConvertIfStatementToReturnStatement" )]
      protected string GetExplorerTooltipValue()
      {
         if (IsAssociationClass)
            return "Association";

         if (IsQueryType)
            return "SQL Query";

         if (!GenerateCode)
            return "Not Generated";

         if (IsPropertyBag)
            return "Dictionary";

         if (!Persistent)
            return "Transient Entity";

         if (IsAbstract)
            return "Abstract Entity";

         if (IsDatabaseView)
            return "View";

         return "Entity";
      }

      /// <summary>
      ///    Gets the glyph type value for the diagram. This determines the visibilities of the class shape decorators
      /// </summary>
      /// <returns>The type of glyph that should be displayed</returns>
      [SuppressMessage( "ReSharper", "ConvertIfStatementToReturnStatement" )]
      protected string GetGlyphTypeValue()
      {
         if (ModelRoot.ShowWarningsInDesigner && GetHasWarningValue())
            return "WarningGlyph";

         if (IsAssociationClass)
            return "AssociationClassGlyph";

         if (IsQueryType)
            return "SQLGlyph";

         if (!GenerateCode)
            return "NoGenGlyph";

         if (IsPropertyBag)
            return "DictionaryGlyph";

         if (!Persistent)
            return "TransientGlyph";

         if (IsAbstract)
            return "AbstractEntityGlyph";

         if (IsDatabaseView)
            return "ViewGlyph";

         return "EntityGlyph";
      }

#endregion

#region Validations

      [ValidationMethod(ValidationCategories.Open | ValidationCategories.Save | ValidationCategories.Menu)]
      [UsedImplicitly]
      [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by validation")]
      private void ClassShouldHaveAttributes(ValidationContext context)
      {
         if (ModelRoot == null)
            return;

         if (!Attributes.Any() && !LocalNavigationProperties().Any())
         {
            context.LogWarning($"{Name}: Class has no properties", "MCWNoProperties", this);
            hasWarning = true;
            RedrawItem();
         }
      }

      [ValidationMethod(ValidationCategories.Menu)]
      [UsedImplicitly]
      [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by validation")]
      private void OwnedTypeCannotHaveABaseClass(ValidationContext context)
      {
         if (ModelRoot == null)
            return;

         if (IsDependentType && (Superclass != null))
            context.LogError($"Can't make {Name} a dependent class since it has a base class", "MCEOwnedHasBaseClass", this);
      }

      [ValidationMethod(ValidationCategories.Menu)]
      [UsedImplicitly]
      [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by validation")]
      private void OwnedTypeCannotHaveASubclass(ValidationContext context)
      {
         if (ModelRoot == null)
            return;

         if (IsDependentType && Subclasses.Any())
            context.LogError($"Can't make {Name} a dependent class since it has subclass(es) {string.Join(", ", Subclasses.Select(s => s.Name))}", "MCEOwnedHasSubclass", this);
      }

      [ValidationMethod(ValidationCategories.Menu)]
      [UsedImplicitly]
      [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by validation")]
      private void OwnedTypeCannotBeAbstract(ValidationContext context)
      {
         if (ModelRoot == null)
            return;

         if (IsDependentType && IsAbstract)
            context.LogError($"Can't make {Name} a dependent class since it's abstract", "MCEOwnedIsAbstract", this);
      }

      [ValidationMethod(ValidationCategories.Menu)]
      [UsedImplicitly]
      [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by validation")]
      private void OwnedTypeCannotBePrincipal(ValidationContext context)
      {
         if (ModelRoot == null)
            return;

         List<Association> principalAssociations = ModelRoot.Store.GetAll<Association>().Where(a => a.Principal == this).ToList();

         if (IsDependentType && principalAssociations.Any())
         {
            string badAssociations = string.Join(", ", principalAssociations.Select(a => a.GetDisplayText()));
            context.LogError($"Can't make {Name} a dependent class since it's the principal end in: {badAssociations}", "MCEOwnedIsPrincipal", this);
         }
      }

      [ValidationMethod(ValidationCategories.Menu)]
      [UsedImplicitly]
      [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by validation")]
      private void OwnedTypeCannotBeInBidirectionalAssociation(ValidationContext context)
      {
         if (ModelRoot == null)
            return;

         if (IsDependentType && !ModelRoot.IsEFCore5Plus && ModelRoot.Store.GetAll<BidirectionalAssociation>().Any(a => (a.Source == this) || (a.Target == this)))
            context.LogError($"Can't make {Name} a dependent class since it's part of a bidirectional association", "MCEOwnedInBidirectional", this);
      }

      [ValidationMethod(ValidationCategories.Save | ValidationCategories.Menu)]
      [UsedImplicitly]
      [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by validation")]
      private void AttributesCannotBeNamedSameAsEnclosingClass(ValidationContext context)
      {
         if (ModelRoot == null)
            return;

         if (HasPropertyNamed(Name))
            context.LogError($"{Name}: Properties can't be named the same as the enclosing class", "MCESameName", this);
      }

      [ValidationMethod(ValidationCategories.Save | ValidationCategories.Menu)]
      [UsedImplicitly]
      [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by validation")]
      private void PersistentClassesMustHaveIdentity(ValidationContext context)
      {
         if (ModelRoot == null)
            return;

         if (!IsDependentType && !IsDatabaseView && !IsQueryType && Persistent && !AllIdentityAttributes.Any() && !IsKeylessType())
            context.LogError($"{Name}: Class has no identity property in inheritance chain", "MCENoIdentity", this);
      }

      [ValidationMethod(ValidationCategories.Open | ValidationCategories.Save | ValidationCategories.Menu)]
      [UsedImplicitly]
      [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by validation")]
      private void DerivedClassesShouldNotHaveIdentity(ValidationContext context)
      {
         if (ModelRoot == null)
            return;

         if (Attributes.Any(x => x.IsIdentity))
         {
            ModelClass modelClass = Superclass;

            while (modelClass != null)
            {
               if (modelClass.Attributes.Any(x => x.IsIdentity))
               {
                  context.LogWarning($"{modelClass.Name}: Identity attribute(s) in derived class {Name} become a composite key", "MCWDerivedIdentity", this);
                  hasWarning = true;
                  RedrawItem();

                  return;
               }

               modelClass = modelClass.Superclass;
            }
         }
      }

      [ValidationMethod(ValidationCategories.Open | ValidationCategories.Save | ValidationCategories.Menu)]
      [UsedImplicitly]
      [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by validation")]
      private void SummaryDescriptionIsEmpty(ValidationContext context)
      {
         if (ModelRoot == null)
            return;

         ModelRoot modelRoot = Store.ElementDirectory.FindElements<ModelRoot>().FirstOrDefault();

         if ((modelRoot?.WarnOnMissingDocumentation == true) && string.IsNullOrWhiteSpace(Summary))
         {
            context.LogWarning($"Class {Name} should be documented", "AWMissingSummary", this);
            hasWarning = true;
            RedrawItem();
         }
      }

#endregion Validations

#region DatabaseSchema tracking property

      private string databaseSchemaStorage;

      private string GetDatabaseSchemaValue()
      {
         if (!this.IsLoading() && IsDatabaseSchemaTracking)
         {
            try
            {
               return Store.ModelRoot()?.DatabaseSchema;
            }
            catch (NullReferenceException)
            {
               return null;
            }
            catch (Exception e)
            {
               if (CriticalException.IsCriticalException(e))
                  throw;

               return null;
            }
         }

         return databaseSchemaStorage;
      }

      private void SetDatabaseSchemaValue(string value)
      {
         databaseSchemaStorage = value;

         if (!Store.InUndoRedoOrRollback && !this.IsLoading())
            IsDatabaseSchemaTracking = false;
      }

      internal sealed partial class IsDatabaseSchemaTrackingPropertyHandler
      {
         /// <summary>
         ///    Called after the IsDatabaseSchemaTracking property changes.
         /// </summary>
         /// <param name="element">The model element that has the property that changed. </param>
         /// <param name="oldValue">The previous value of the property. </param>
         /// <param name="newValue">The new value of the property. </param>
         protected override void OnValueChanged(ModelClass element, bool oldValue, bool newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);

            if (!element.Store.InUndoRedoOrRollback && newValue)
            {
               DomainPropertyInfo propInfo = element.Store.DomainDataDirectory.GetDomainProperty(DatabaseSchemaDomainPropertyId);
               propInfo.NotifyValueChange(element);
            }
         }

         /// <summary>
         ///    Method to set IsDatabaseSchemaTracking to false so that this instance of this tracking property is not
         ///    storage-based.
         /// </summary>
         /// <param name="element">
         ///    The element on which to reset the property
         ///    value.
         /// </param>
         internal void PreResetValue(ModelClass element)
         {
            // of the DatabaseSchema property is retrieved from storage.  
            // Force the IsDatabaseSchemaTracking property to false so that the value  
            element.isDatabaseSchemaTrackingPropertyStorage = false;
         }

         /// <summary>Performs the reset operation for the IsDatabaseSchemaTracking property for a model element.</summary>
         /// <param name="element">The model element that has the property to reset.</param>
         internal void ResetValue(ModelClass element)
         {
            object calculatedValue = null;
            ModelRoot modelRoot = element.Store.ModelRoot();

            try
            {
               calculatedValue = modelRoot?.DatabaseSchema;
            }
            catch (NullReferenceException) { }
            catch (Exception e)
            {
               if (CriticalException.IsCriticalException(e))
                  throw;
            }

            if ((calculatedValue != null) && (element.DatabaseSchema == (string)calculatedValue))
               element.isDatabaseSchemaTrackingPropertyStorage = true;
         }
      }

#endregion DatabaseSchema tracking property

#region DefaultConstructorVisibility tracking property

      private TypeAccessModifierExt defaultConstructorVisibilityStorage;

      private TypeAccessModifierExt GetDefaultConstructorVisibilityValue()
      {
         if (!this.IsLoading() && IsDefaultConstructorVisibilityTracking)
         {
            try
            {
               return Store.ModelRoot()?.EntityDefaultConstructorVisibilityDefault ?? TypeAccessModifierExt.Default;
            }
            catch (NullReferenceException)
            {
               return TypeAccessModifierExt.Default;
            }
            catch (Exception e)
            {
               if (CriticalException.IsCriticalException(e))
                  throw;

               return TypeAccessModifierExt.Default;
            }
         }

         return defaultConstructorVisibilityStorage;
      }

      private void SetDefaultConstructorVisibilityValue(TypeAccessModifierExt value)
      {
         defaultConstructorVisibilityStorage = value;

         if (!Store.InUndoRedoOrRollback && !this.IsLoading())
            IsDefaultConstructorVisibilityTracking = false;
      }

      internal sealed partial class IsDefaultConstructorVisibilityTrackingPropertyHandler
      {
         /// <summary>
         ///    Called after the IsDefaultConstructorVisibilityTracking property changes.
         /// </summary>
         /// <param name="element">The model element that has the property that changed. </param>
         /// <param name="oldValue">The previous value of the property. </param>
         /// <param name="newValue">The new value of the property. </param>
         protected override void OnValueChanged(ModelClass element, bool oldValue, bool newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);

            if (!element.Store.InUndoRedoOrRollback && newValue)
            {
               DomainPropertyInfo propInfo = element.Store.DomainDataDirectory.GetDomainProperty(DefaultConstructorVisibilityDomainPropertyId);
               propInfo.NotifyValueChange(element);
            }
         }

         /// <summary>
         ///    Method to set IsDefaultConstructorVisibilityTracking to false so that this instance of this tracking property is not
         ///    storage-based.
         /// </summary>
         /// <param name="element">
         ///    The element on which to reset the property value.
         /// </param>
         internal void PreResetValue(ModelClass element)
         {
            // of the DefaultConstructorVisibility property is retrieved from storage.  
            // Force the IsDefaultConstructorVisibilityTracking property to false so that the value  
            element.isDefaultConstructorVisibilityTrackingPropertyStorage = false;
         }

         /// <summary>Performs the reset operation for the IsDefaultConstructorVisibilityTracking property for a model element.</summary>
         /// <param name="element">The model element that has the property to reset.</param>
         internal void ResetValue(ModelClass element)
         {
            object calculatedValue = null;
            ModelRoot modelRoot = element.Store.ModelRoot();

            try
            {
               calculatedValue = modelRoot?.EntityDefaultConstructorVisibilityDefault;
            }
            catch (NullReferenceException) { }
            catch (Exception e)
            {
               if (CriticalException.IsCriticalException(e))
                  throw;
            }

            if ((calculatedValue != null) && (element.DefaultConstructorVisibility == (TypeAccessModifierExt)calculatedValue))
               element.isDefaultConstructorVisibilityTrackingPropertyStorage = true;
         }
      }

#endregion

#region Namespace tracking property

      private string namespaceStorage;

      private string GetNamespaceValue()
      {
         if (!this.IsLoading() && IsNamespaceTracking)
         {
            try
            {
               return DefaultNamespace;
            }
            catch (NullReferenceException)
            {
               return null;
            }
            catch (Exception e)
            {
               if (CriticalException.IsCriticalException(e))
                  throw;

               return null;
            }
         }

         return namespaceStorage;
      }

      private void SetNamespaceValue(string value)
      {
         namespaceStorage = string.IsNullOrWhiteSpace(value) || (value == DefaultNamespace)
                               ? null
                               : value;

         if (!Store.InUndoRedoOrRollback && !this.IsLoading())
            IsNamespaceTracking = namespaceStorage == null;
      }

      internal sealed partial class IsNamespaceTrackingPropertyHandler
      {
         /// <summary>
         ///    Called after the IsNamespaceTracking property changes.
         /// </summary>
         /// <param name="element">The model element that has the property that changed. </param>
         /// <param name="oldValue">The previous value of the property. </param>
         /// <param name="newValue">The new value of the property. </param>
         protected override void OnValueChanged(ModelClass element, bool oldValue, bool newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);

            if (!element.Store.InUndoRedoOrRollback && newValue)
            {
               DomainPropertyInfo propInfo = element.Store.DomainDataDirectory.GetDomainProperty(NamespaceDomainPropertyId);
               propInfo.NotifyValueChange(element);
            }
         }

         /// <summary>
         ///    Method to set IsNamespaceTracking to false so that this instance of this tracking property is not
         ///    storage-based.
         /// </summary>
         /// <param name="element">
         ///    The element on which to reset the property
         ///    value.
         /// </param>
         internal void PreResetValue(ModelClass element)
         {
            // of the Namespace property is retrieved from storage.  
            // Force the IsNamespaceTracking property to false so that the value  
            element.isNamespaceTrackingPropertyStorage = false;
         }

         /// <summary>Performs the reset operation for the IsNamespaceTracking property for a model element.</summary>
         /// <param name="element">The model element that has the property to reset.</param>
         internal void ResetValue(ModelClass element)
         {
            element.isNamespaceTrackingPropertyStorage = string.IsNullOrWhiteSpace(element.namespaceStorage);
         }
      }

#endregion Namespace tracking property

#region OutputDirectory tracking property

      private string outputDirectoryStorage;

      private string GetOutputDirectoryValue()
      {
         if (!this.IsLoading() && IsOutputDirectoryTracking)
         {
            try
            {
               return DefaultOutputDirectory;
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

         return outputDirectoryStorage;
      }

      private void SetOutputDirectoryValue(string value)
      {
         outputDirectoryStorage = string.IsNullOrWhiteSpace(value) || (value == DefaultOutputDirectory)
                                     ? null
                                     : value;

         if (!Store.InUndoRedoOrRollback && !this.IsLoading())
            IsOutputDirectoryTracking = outputDirectoryStorage == null;
      }

      internal sealed partial class IsOutputDirectoryTrackingPropertyHandler
      {
         /// <summary>
         ///    Called after the IsOutputDirectoryTracking property changes.
         /// </summary>
         /// <param name="element">The model element that has the property that changed. </param>
         /// <param name="oldValue">The previous value of the property. </param>
         /// <param name="newValue">The new value of the property. </param>
         protected override void OnValueChanged(ModelClass element, bool oldValue, bool newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);

            if (!element.Store.InUndoRedoOrRollback && newValue)
            {
               DomainPropertyInfo propInfo = element.Store.DomainDataDirectory.GetDomainProperty(OutputDirectoryDomainPropertyId);
               propInfo.NotifyValueChange(element);
            }
         }

         /// <summary>
         ///    Method to set IsOutputDirectoryTracking to false so that this instance of this tracking property is not
         ///    storage-based.
         /// </summary>
         /// <param name="element">
         ///    The element on which to reset the property value.
         /// </param>
         internal void PreResetValue(ModelClass element)
         {
            // of the OutputDirectory property is retrieved from storage.  
            // Force the IsOutputDirectoryTracking property to false so that the value  
            element.isOutputDirectoryTrackingPropertyStorage = false;
         }

         /// <summary>Performs the reset operation for the IsOutputDirectoryTracking property for a model element.</summary>
         /// <param name="element">The model element that has the property to reset.</param>
         internal void ResetValue(ModelClass element)
         {
            object calculatedValue = null;
            ModelRoot modelRoot = element.Store.ModelRoot();

            try
            {
               calculatedValue = element.IsDependentType
                                    ? modelRoot?.StructOutputDirectory
                                    : element.ModelRoot?.EntityOutputDirectory;
            }
            catch (NullReferenceException) { }
            catch (Exception e)
            {
               if (CriticalException.IsCriticalException(e))
                  throw;
            }

            if ((calculatedValue != null) && (element.OutputDirectory == (string)calculatedValue))
               element.isOutputDirectoryTrackingPropertyStorage = true;
         }
      }

#endregion OutputDirectory tracking property

#region IsImplementNotify tracking property

      /// <summary>
      ///    Updates tracking properties when the IsImplementNotify value changes
      /// </summary>
      /// <param name="oldValue">Prior value</param>
      /// <param name="newValue">Current value</param>
      protected virtual void OnIsImplementNotifyChanged(bool oldValue, bool newValue)
      {
         TrackingHelper.UpdateTrackingCollectionProperty(Store,
                                                         Attributes,
                                                         ModelAttribute.ImplementNotifyDomainPropertyId,
                                                         ModelAttribute.IsImplementNotifyTrackingDomainPropertyId);

         TrackingHelper.UpdateTrackingCollectionProperty(Store,
                                                         Store.GetAll<Association>().Where(a => a.Source?.FullName == FullName),
                                                         Association.TargetImplementNotifyDomainPropertyId,
                                                         Association.IsTargetImplementNotifyTrackingDomainPropertyId);

         TrackingHelper.UpdateTrackingCollectionProperty(Store,
                                                         Store.GetAll<BidirectionalAssociation>().Where(a => a.Target?.FullName == FullName),
                                                         BidirectionalAssociation.SourceImplementNotifyDomainPropertyId,
                                                         BidirectionalAssociation.IsSourceImplementNotifyTrackingDomainPropertyId);
      }

      internal sealed partial class ImplementNotifyPropertyHandler
      {
         protected override void OnValueChanged(ModelClass element, bool oldValue, bool newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);

            if (!element.Store.InUndoRedoOrRollback)
               element.OnIsImplementNotifyChanged(oldValue, newValue);
         }
      }

#endregion IsImplementNotify tracking property

#region AutoPropertyDefault tracking property

      // this property is both a tracking property (dependent on ModelRoot.AutoPropertyDefault)
      // and a tracked property (ModelAttribute.AutoProperty, Association.TargetAutoProperty and BidirectionalAssociation.SourceAutoProperty depends on it)

      private bool autoPropertyDefaultStorage;

      /// <summary>Gets the storage for the AutoPropertyDefault property.</summary>
      /// <returns>The AutoPropertyDefault value.</returns>
      public bool GetAutoPropertyDefaultValue()
      {
         if (!this.IsLoading() && IsAutoPropertyDefaultTracking)
         {
            try
            {
               return ModelRoot?.AutoPropertyDefault ?? true;
            }
            catch (NullReferenceException)
            {
               return true;
            }
            catch (Exception e)
            {
               if (CriticalException.IsCriticalException(e))
                  throw;

               return true;
            }
         }

         return autoPropertyDefaultStorage;
      }

      /// <summary>Sets the storage for the AutoPropertyDefault property.</summary>
      /// <param name="value">The AutoPropertyDefault value.</param>
      public void SetAutoPropertyDefaultValue(bool value)
      {
         autoPropertyDefaultStorage = value;

         if (!Store.InUndoRedoOrRollback && !this.IsLoading())
            IsAutoPropertyDefaultTracking = autoPropertyDefaultStorage == (ModelRoot?.AutoPropertyDefault ?? true);
      }

      /// <summary>
      ///    Updates tracking properties when the AutoPropertyDefault value changes
      /// </summary>
      /// <param name="oldValue">Prior value</param>
      /// <param name="newValue">Current value</param>
      protected virtual void OnAutoPropertyDefaultChanged(bool oldValue, bool newValue)
      {
         TrackingHelper.UpdateTrackingCollectionProperty(Store,
                                                         Attributes,
                                                         ModelAttribute.AutoPropertyDomainPropertyId,
                                                         ModelAttribute.IsAutoPropertyTrackingDomainPropertyId);

         TrackingHelper.UpdateTrackingCollectionProperty(Store,
                                                         Store.GetAll<Association>().Where(a => a.Source?.FullName == FullName),
                                                         Association.TargetAutoPropertyDomainPropertyId,
                                                         Association.IsTargetAutoPropertyTrackingDomainPropertyId);

         TrackingHelper.UpdateTrackingCollectionProperty(Store,
                                                         Store.GetAll<BidirectionalAssociation>().Where(a => a.Target?.FullName == FullName),
                                                         BidirectionalAssociation.SourceAutoPropertyDomainPropertyId,
                                                         BidirectionalAssociation.IsSourceAutoPropertyTrackingDomainPropertyId);
      }

      internal sealed partial class AutoPropertyDefaultPropertyHandler
      {
         protected override void OnValueChanged(ModelClass element, bool oldValue, bool newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);

            if (!element.Store.InUndoRedoOrRollback)
               element.OnAutoPropertyDefaultChanged(oldValue, newValue);
         }
      }

#endregion AutoPropertyDefault tracking property
   }
}