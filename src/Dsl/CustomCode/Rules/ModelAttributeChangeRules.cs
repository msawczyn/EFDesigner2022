using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.Modeling;

using Sawczyn.EFDesigner.EFModel.Extensions;

namespace Sawczyn.EFDesigner.EFModel
{
   [RuleOn(typeof(ModelAttribute), FireTime = TimeToFire.TopLevelCommit)]
   internal class ModelAttributeChangeRules : ChangeRule
   {
      /// <inheritdoc />
      public override void ElementPropertyChanged(ElementPropertyChangedEventArgs e)
      {
         base.ElementPropertyChanged(e);

         ModelAttribute element = (ModelAttribute)e.ModelElement;

         if (element.IsDeleted)
            return;

         ModelClass modelClass = element.ModelClass;
         ModelRoot modelRoot = element.Store.ModelRoot();

         Store store = element.Store;
         Transaction current = store.TransactionManager.CurrentTransaction;

         if (current.IsSerializing || ModelRoot.BatchUpdating)
            return;

         if (Equals(e.NewValue, e.OldValue))
            return;

         List<string> errorMessages = new List<string>();

         switch (e.DomainProperty.Name)
         {
            case "AutoProperty":
               {
                  if (element.AutoProperty)
                  {
                     //element.PersistencePoint = PersistencePointType.Property;
                     element.ImplementNotify = false;
                  }
                  else
                  {
                     if (string.IsNullOrEmpty(element.BackingFieldName))
                        element.BackingFieldName = $"_{element.Name.ToCamelCase()}";
                  }
               }

               break;

            case "BackingFieldName":
               {
                  if (modelRoot.ReservedWords.Contains(element.BackingFieldName))
                     element.BackingFieldName = "@" + element.BackingFieldName;
               }

               break;

            case "IdentityType":
               {
                  if (element.IsIdentity)
                  {
                     if (element.IdentityType == IdentityType.None)
                        errorMessages.Add($"{modelClass.Name}.{element.Name}: Identity properties must have an identity type defined");

                     foreach (Association association in element.ModelClass.LocalNavigationProperties()
                                                                .Where(nav => nav.AssociationObject.Dependent == element.ModelClass)
                                                                .Select(nav => nav.AssociationObject)
                                                                .Where(a => !string.IsNullOrWhiteSpace(a.FKPropertyName)
                                                                         && a.FKPropertyName.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Any(n => n.Trim() == element.Name)))
                     {
                        foreach (ModelAttribute attribute in association.GetFKAutoIdentityErrors())
                           errorMessages.Add($"{association.Source.Name} <=> {association.Target.Name}: FK property {attribute.Name} in {association.Dependent.FullName} is an auto-generated identity. Migration will fail.");
                     }
                  }
                  else
                     element.IdentityType = IdentityType.None;
               }

               break;

            case "ImplementNotify":
               {
                  if (element.IsIdentity)
                     element.ImplementNotify = false;

                  if (element.ImplementNotify)
                     element.AutoProperty = false;
               }

               break;

            case "Indexed":
               {
                  if (element.IsIdentity)
                     element.Indexed = true;

                  if (element.IsConcurrencyToken)
                  {
                     element.Indexed = false;
                     element.Required = false;
                  }

                  if (element.Indexed)
                     element.Persistent = true;
               }

               break;

            case "InitialValue":
               {
                  string newInitialValue = (string)e.NewValue;

                  if (string.IsNullOrEmpty(newInitialValue))
                     break;

                  // if the property is an Enum and the user just typed the name of the Enum value without the Enum type name, help them out
                  if (element.ModelClass.ModelRoot.Enums.Any(x => x.Name == element.Type) && !newInitialValue.Contains("."))
                     newInitialValue = element.InitialValue = $"{element.Type}.{newInitialValue}";

                  if (!element.IsValidInitialValue(null, newInitialValue))
                     errorMessages.Add($"{modelClass.Name}.{element.Name}: {newInitialValue} isn't a valid value for {element.Type}");
               }

               break;

            case "IsAbstract":
               {
                  if ((bool)e.NewValue)
                     modelClass.IsAbstract = true;
               }

               break;

            case "IsConcurrencyToken":
               {
                  if (element.IsConcurrencyToken)
                  {
                     if (element.IsIdentity)
                        errorMessages.Add($"{modelClass.Name}.{element.Name}: Makes no sense for a concurrenty token to be an identity.");
                     else
                     {
                        foreach (ModelAttribute modelAttribute in element.ModelClass.AllAttributes.Where(a => a.IsConcurrencyToken && a != element))
                           modelAttribute.IsConcurrencyToken = false;

                        if (element.Type == "Timestamp")
                           element.Required = false;
                     }
                  }
               }

               break;

            case "IsIdentity":
               {
                  if ((bool)e.NewValue)
                  {
                     if (element.IsConcurrencyToken)
                        errorMessages.Add($"{modelClass.Name}.{element.Name}: Makes no sense for a concurrenty token to be an identity.");
                     else if (element.ModelClass.IsDependentType)
                     {
                        if (!modelRoot.IsEFCore5Plus)
                           errorMessages.Add($"{modelClass.Name}.{element.Name}: Can't make {element.Name} an identity because {modelClass.Name} is a dependent type and can't have an identity property.");
                     }
                     else
                     {
                        if (!modelRoot.IsValidIdentityAttributeType(element.Type))
                           errorMessages.Add($"{modelClass.Name}.{element.Name}: Properties of type {element.Type} can't be used as identity properties.");
                        else
                        {
                           element.IsConcurrencyToken = false;
                           element.Indexed = true;
                           element.IndexedUnique = true;
                           element.Persistent = true;
                           element.Required = true;

                           if (element.IsForeignKeyProperty)
                              element.IdentityType = IdentityType.Manual;
                           else if (element.IdentityType == IdentityType.None)
                              element.IdentityType = IdentityType.AutoGenerated;
                        }
                     }

                     // looks like something got started here but never finished. Doesn't matter as this validation is implemented elsewhere
                     //foreach (Association association in element.ModelClass.LocalNavigationProperties()
                     //                                           .Where(nav => nav.AssociationObject.Dependent == element.ModelClass)
                     //                                           .Select(nav => nav.AssociationObject)
                     //                                           .Where(a => !string.IsNullOrWhiteSpace(a.FKPropertyName)
                     //                                                    && a.FKPropertyName.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Any(n => n.Trim() == element.Name)))
                     //   association.GetFKAutoIdentityErrors();
                  }
                  else
                     element.IdentityType = IdentityType.None;
               }

               break;

            case "MinLength":
               {
                  int minLengthValue = (int)e.NewValue;

                  if (element.Type != "String")
                     element.MinLength = 0;
                  else if (minLengthValue < 0)
                     errorMessages.Add($"{modelClass.Name}.{element.Name}: MinLength must be zero or a positive number");
                  else if ((element.MaxLength > 0) && (minLengthValue > element.MaxLength))
                     errorMessages.Add($"{modelClass.Name}.{element.Name}: MinLength cannot be greater than MaxLength");
               }

               break;

            case "MaxLength":
               {
                  if (element.Type != "String")
                     element.MaxLength = null;
                  else
                  {
                     int? maxLengthValue = (int?)e.NewValue;

                     if ((maxLengthValue > 0) && (element.MinLength > maxLengthValue))
                        errorMessages.Add($"{modelClass.Name}.{element.Name}: MinLength cannot be greater than MaxLength");
                  }
               }

               break;

            case "Name":
               {
                  if (string.IsNullOrEmpty(element.Name) || !CodeGenerator.IsValidLanguageIndependentIdentifier(element.Name))
                     errorMessages.Add($"{modelClass.Name}: Property name '{element.Name}' isn't a valid .NET identifier");

                  if (modelRoot.ReservedWords.Contains(element.Name))
                     element.Name = "@" + element.Name;

                  if (modelClass.AllAttributes.Except(new[] { element }).Any(x => x.Name == element.Name)
                   || modelClass.AllNavigationProperties().Any(p => p.PropertyName == element.Name))
                     errorMessages.Add($"{modelClass.Name}: Property name '{element.Name}' already in use");
               }

               break;

            case "Persistent":
               {
                  element.Persistent = (bool)e.NewValue && (element.ModelClass?.Persistent ?? true);

                  if (!element.Persistent)
                  {
                     element.IsIdentity = false;
                     element.Indexed = false;
                     element.IndexedUnique = false;
                     element.IdentityType = IdentityType.None;
                     element.IsConcurrencyToken = false;
                     element.Virtual = false;
                  }
               }

               break;

            case "ReadOnly":
               {
                  // only transient properties can be read-only
                  // and it makes no sense for a non-public property to be read-only
                  if (element.Persistent || element.SetterVisibility != SetterAccessModifier.Public)
                     element.ReadOnly = false;
               }

               break;

            case "Required":
               {
                  if (element.IsConcurrencyToken)
                     element.Required = false;
                  else if (element.IsIdentity)
                     element.Required = true;
               }

               break;

            case "Type":
               {
                  string newType = (string)e.NewValue;

                  if (element.IsIdentity)
                  {
                     if (!modelRoot.IsValidIdentityAttributeType(ModelAttribute.ToCLRType(newType)))
                        errorMessages.Add($"{modelClass.Name}.{element.Name}: Properties of type {newType} can't be used as identity properties.");
                     else
                     {
                        element.Required = true;
                        element.Persistent = true;

                        // Change type of any foreign key pointing to this class
                        IEnumerable<Association> participatingAssociations =
                           element.ModelClass.LocalNavigationProperties()
                                  .Where(nav => nav.AssociationObject.Dependent == element.ModelClass)
                                  .Select(nav => nav.AssociationObject)
                                  .Where(a => !string.IsNullOrWhiteSpace(a.FKPropertyName)
                                           && a.FKPropertyName.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Any(n => n.Trim() == element.Name));

                        foreach (Association association in participatingAssociations)
                           AssociationChangedRules.FixupForeignKeys(association);
                     }
                  }

                  if (newType != "String")
                  {
                     element.MaxLength = null;
                     element.MinLength = 0;
                     element.StringType = HTML5Type.None;
                  }
                  else
                  {
                     if (!element.IsValidInitialValue(newType))
                        element.InitialValue = null;
                  }

                  if (element.IsConcurrencyToken)
                     element.Type = "Binary";

                  if (!element.SupportsInitialValue)
                     element.InitialValue = null;
               }

               break;
            case "TypePrecision":
               {
                  if (modelRoot.IsEFCore6Plus && !string.IsNullOrEmpty(element.TypePrecision) && (element.Type == "decimal" || element.Type == "Decimal"))
                  {
                     if (!int.TryParse(element.TypePrecision, out _))
                        errorMessages.Add($"{modelClass.Name}.{element.Name}: {e.NewValue} isn't a valid value for Precision. It must be an integer.");
                  }
               }

               break;
            case "TypeScale":
               {
                  if (modelRoot.IsEFCore6Plus && !string.IsNullOrEmpty(element.TypeScale) && (element.Type == "decimal" || element.Type == "Decimal"))
                  {
                     if (!int.TryParse(element.TypeScale, out _))
                        errorMessages.Add($"{modelClass.Name}.{element.Name}: {e.NewValue} isn't a valid value for Scale. It must be an integer.");
                  }
               }

               break;
         }

         errorMessages = errorMessages.Where(m => m != null).ToList();

         if (errorMessages.Any())
         {
            current.Rollback();
            ErrorDisplay.Show(store, string.Join("\n", errorMessages));
         }
      }
   }
}