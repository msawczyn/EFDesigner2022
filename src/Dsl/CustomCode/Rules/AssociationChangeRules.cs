using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.Modeling;

using Sawczyn.EFDesigner.EFModel.Extensions;

namespace Sawczyn.EFDesigner.EFModel
{
   [RuleOn(typeof(Association), FireTime = TimeToFire.TopLevelCommit)]
   internal class AssociationChangedRules : ChangeRule
   {
      // matches [Display(Name="*")] and [System.ComponentModel.DataAnnotations.Display(Name="*")]
      private static readonly Regex DisplayAttributeRegex = new Regex("^(.*)\\[(System\\.ComponentModel\\.DataAnnotations\\.)?Display\\(Name=\"([^\"]+)\"\\)\\](.*)$", RegexOptions.Compiled);

      private static void CheckSourceForDisplayText(BidirectionalAssociation bidirectionalAssociation)
      {
         Match match = DisplayAttributeRegex.Match(bidirectionalAssociation.SourceCustomAttributes);

         // is there a custom attribute for [Display]?
         if (match != Match.Empty)
         {
            // if SourceDisplayText is empty, move the Name down to SourceDisplayText
            if (string.IsNullOrWhiteSpace(bidirectionalAssociation.SourceDisplayText))
               bidirectionalAssociation.SourceDisplayText = match.Groups[3].Value;

            // if custom attribute's Name matches SourceDisplayText, remove that attribute, leaving other custom attributes if present
            if (match.Groups[3].Value == bidirectionalAssociation.SourceDisplayText)
               bidirectionalAssociation.SourceCustomAttributes = match.Groups[1].Value + match.Groups[4].Value;
         }
      }

      private static void CheckTargetForDisplayText(Association association)
      {
         Match match = DisplayAttributeRegex.Match(association.TargetCustomAttributes);

         // is there a custom attribute for [Display]?
         if (match != Match.Empty)
         {
            // if TargetDisplayText is empty, move the Name down to TargetDisplayText
            if (string.IsNullOrWhiteSpace(association.TargetDisplayText))
               association.TargetDisplayText = match.Groups[3].Value;

            // if custom attribute's Name matches TargetDisplayText, remove that attribute, leaving other custom attributes if present
            if (match.Groups[3].Value == association.TargetDisplayText)
               association.TargetCustomAttributes = match.Groups[1].Value + match.Groups[4].Value;
         }
      }

      /// <inheritdoc />
      public override void ElementPropertyChanged(ElementPropertyChangedEventArgs e)
      {
         base.ElementPropertyChanged(e);

         Association element = (Association)e.ModelElement;

         if (element.IsDeleted)
            return;

         Store store = element.Store;
         Transaction current = store.TransactionManager.CurrentTransaction;

         if (current.IsSerializing || ModelRoot.BatchUpdating)
            return;

         if (Equals(e.NewValue, e.OldValue))
            return;

         List<string> errorMessages = EFCoreValidator.GetErrors(element).ToList();
         BidirectionalAssociation bidirectionalAssociation = element as BidirectionalAssociation;

         using (Transaction inner = store.TransactionManager.BeginTransaction("Association ElementPropertyChanged"))
         {
            bool doForeignKeyFixup = false;

            switch (e.DomainProperty.Name)
            {
               case "FKPropertyName":
                  {
                     if ((store.ModelRoot().EntityFrameworkVersion == EFVersion.EF6)
                      && (element.SourceMultiplicity != Multiplicity.ZeroMany)
                      && (element.TargetMultiplicity != Multiplicity.ZeroMany))
                     {
                        element.FKPropertyName = null;
                        doForeignKeyFixup = true;
                     }
                     else
                        ValidateForeignKeyNames(element, errorMessages);

                     if (!errorMessages.Any())
                        doForeignKeyFixup = true;

                     break;
                  }

               case "Persistent":
                  {
                     if (element.Source.Persistent ^ element.Target.Persistent)
                     {
                        if (element.Target.Persistent && !element.Source.Persistent && element.SourceRole != EndpointRole.Dependent)
                        {
                           // source is transient, target is persistent. Source must be dependent
                           errorMessages.Add($"Unsupported association between {element.Source.Name} and {element.Target.Name}. The transient class must be the Dependent.");
                        }
                        else if (element.Source.Persistent && !element.Target.Persistent && element.TargetRole != EndpointRole.Dependent)
                        {
                           // source is persistent, target is transient. Target must be dependent
                           errorMessages.Add($"Unsupported association between {element.Source.Name} and {element.Target.Name}. The transient class must be the Dependent.");
                        }
                     }

                     break;
                  }

               case "SourceCustomAttributes":
                  {
                     if ((bidirectionalAssociation != null) && !string.IsNullOrWhiteSpace(bidirectionalAssociation.SourceCustomAttributes))
                     {
                        bidirectionalAssociation.SourceCustomAttributes = $"[{bidirectionalAssociation.SourceCustomAttributes.Trim('[', ']')}]";
                        CheckSourceForDisplayText(bidirectionalAssociation);
                     }

                     break;
                  }

               case "SourceDisplayText":
                  {
                     if (bidirectionalAssociation != null)
                        CheckSourceForDisplayText(bidirectionalAssociation);

                     break;
                  }

               case "SourceMultiplicity":
                  {
                     if (element.AllCardinalitiesAreValid(out string errorMessage, ref doForeignKeyFixup))
                     {
                        Multiplicity priorSourceMultiplicity = (Multiplicity)e.OldValue;

                        if ((((priorSourceMultiplicity == Multiplicity.ZeroOne) || (priorSourceMultiplicity == Multiplicity.ZeroMany)) && (element.SourceMultiplicity == Multiplicity.One))
                         || (((element.SourceMultiplicity == Multiplicity.ZeroOne) || (element.SourceMultiplicity == Multiplicity.ZeroMany)) && (priorSourceMultiplicity == Multiplicity.One)))
                           doForeignKeyFixup = true;

                        string defaultSourcePropertyName =
                           (priorSourceMultiplicity == Multiplicity.ZeroMany) && (ModelRoot.PluralizationService?.IsSingular(element.Source.Name) == true)
                              ? ModelRoot.PluralizationService.Pluralize(element.Source.Name)
                              : element.Source.Name;

                        if (element is BidirectionalAssociation bidirectional && (bidirectional.SourcePropertyName == defaultSourcePropertyName))
                        {
                           bidirectional.SourcePropertyName = (element.SourceMultiplicity == Multiplicity.ZeroMany) && (ModelRoot.PluralizationService?.IsSingular(element.Source.Name) == true)
                                                                 ? ModelRoot.PluralizationService.Pluralize(element.Source.Name)
                                                                 : element.Source.Name;
                        }
                     }
                     else
                        errorMessages.Add(errorMessage);

                     if (element.Dependent?.IsKeyless() == true)
                        errorMessages.Add($"{element.Dependent.Name} is keyless. Keyless entities cannot be the dependent in an association.");

                     break;
                  }

               case "SourcePropertyName":
                  {
                     string sourcePropertyNameErrorMessage = ValidateAssociationIdentifier(element, element.Target, (string)e.NewValue);

                     if (EFModelDiagram.IsDroppingExternal && (sourcePropertyNameErrorMessage != null))
                        element.Delete();
                     else
                        errorMessages.Add(sourcePropertyNameErrorMessage);

                     break;
                  }

               case "SourceRole":
                  {
                     if (element.SourceRole == EndpointRole.NotApplicable)
                        element.SourceRole = EndpointRole.NotSet;

                     if (!element.SetEndpointRoles())
                     {
                        if ((element.SourceRole == EndpointRole.Dependent) && (element.TargetRole != EndpointRole.Principal))
                           element.TargetRole = EndpointRole.Principal;
                        else if ((element.SourceRole == EndpointRole.Principal) && (element.TargetRole != EndpointRole.Dependent))
                           element.TargetRole = EndpointRole.Dependent;
                     }

                     doForeignKeyFixup = true;

                     break;
                  }

               case "TargetCustomAttributes":
                  {
                     if (!string.IsNullOrWhiteSpace(element.TargetCustomAttributes))
                     {
                        element.TargetCustomAttributes = $"[{element.TargetCustomAttributes.Trim('[', ']')}]";
                        CheckTargetForDisplayText(element);
                     }

                     break;
                  }

               case "TargetDisplayText":
                  {
                     CheckTargetForDisplayText(element);

                     break;
                  }

               case "TargetMultiplicity":
                  {
                     if (element.AllCardinalitiesAreValid(out string errorMessage, ref doForeignKeyFixup))
                     {
                        Multiplicity priorTargetMultiplicity = (Multiplicity)e.OldValue;

                        if ((((priorTargetMultiplicity == Multiplicity.ZeroOne) || (priorTargetMultiplicity == Multiplicity.ZeroMany)) && (element.TargetMultiplicity == Multiplicity.One))
                         || (((element.TargetMultiplicity == Multiplicity.ZeroOne) || (element.TargetMultiplicity == Multiplicity.ZeroMany)) && (priorTargetMultiplicity == Multiplicity.One)))
                           doForeignKeyFixup = true;

                        string defaultTargetPropertyName =
                           (priorTargetMultiplicity == Multiplicity.ZeroMany) && (ModelRoot.PluralizationService?.IsSingular(element.Target.Name) == true)
                              ? ModelRoot.PluralizationService.Pluralize(element.Target.Name)
                              : element.Target.Name;

                        if (element.TargetPropertyName == defaultTargetPropertyName)
                        {
                           element.TargetPropertyName = (element.TargetMultiplicity == Multiplicity.ZeroMany) && (ModelRoot.PluralizationService?.IsSingular(element.Target.Name) == true)
                                                           ? ModelRoot.PluralizationService.Pluralize(element.Target.Name)
                                                           : element.Target.Name;
                        }
                     }
                     else
                        errorMessages.Add(errorMessage);

                     break;
                  }

               case "TargetPropertyName":
                  {
                     // if we're creating an association via drag/drop, it's possible the existing property name
                     // is the same as the default property name. The default doesn't get created until the transaction is 
                     // committed, so the drop's action will cause a name clash. Remove the clashing property, but
                     // only if drag/drop.

                     string targetPropertyNameErrorMessage = ValidateAssociationIdentifier(element, element.Source, (string)e.NewValue);

                     if (EFModelDiagram.IsDroppingExternal && (targetPropertyNameErrorMessage != null))
                        element.Delete();
                     else
                        errorMessages.Add(targetPropertyNameErrorMessage);

                     break;
                  }

               case "TargetRole":
                  {
                     if (element.TargetRole == EndpointRole.NotApplicable)
                        element.TargetRole = EndpointRole.NotSet;

                     //if (element.Target.IsDependentType && (element.SourceRole != EndpointRole.Principal || element.TargetRole != EndpointRole.Dependent))
                     //{
                     //   element.SourceRole = EndpointRole.Principal;
                     //   element.TargetRole = EndpointRole.Dependent;
                     //   doForeignKeyFixup = true;
                     //}
                     //else 
                     if (!element.SetEndpointRoles())
                     {
                        if ((element.TargetRole == EndpointRole.Dependent) && (element.SourceRole != EndpointRole.Principal))
                        {
                           element.SourceRole = EndpointRole.Principal;
                           doForeignKeyFixup = true;
                        }
                        else if ((element.TargetRole == EndpointRole.Principal) && (element.SourceRole != EndpointRole.Dependent))
                        {
                           element.SourceRole = EndpointRole.Dependent;
                           doForeignKeyFixup = true;
                        }
                     }

                     break;
                  }
            }

            if (doForeignKeyFixup)
               element.FixupForeignKeys();

            inner.Commit();
            element.RedrawItem();
         }

         errorMessages = errorMessages.Where(m => m != null).ToList();

         if (errorMessages.Any())
         {
            current.Rollback();
            ErrorDisplay.Show(store, string.Join("\n", errorMessages));
         }
      }



      private static string ValidateAssociationIdentifier(Association association, ModelClass targetedClass, string identifier)
      {
         if (string.IsNullOrWhiteSpace(identifier) || !CodeGenerator.IsValidLanguageIndependentIdentifier(identifier))
            return $"{identifier} isn't a valid .NET identifier";

         ModelClass offendingModelClass = targetedClass.AllAttributes.FirstOrDefault(x => x.Name == identifier)?.ModelClass
                                       ?? targetedClass.AllNavigationProperties(association).FirstOrDefault(x => x.PropertyName == identifier)?.ClassType;

         return offendingModelClass != null
                   ? $"Duplicate symbol {identifier} in {offendingModelClass.Name}"
                   : null;
      }

      private static void ValidateForeignKeyNames(Association element, List<string> errorMessages)
      {
         if (!string.IsNullOrWhiteSpace(element.FKPropertyName))
         {
            string[] foreignKeyPropertyNames = element.GetForeignKeyPropertyNames();
            string tag = $"({element.Source.Name}:{element.Target.Name})";

            if (element.Dependent == null)
            {
               errorMessages.Add($"{tag} can't have foreign keys defined; no dependent role found");

               return;
            }

            int propertyCount = foreignKeyPropertyNames.Length;
            int identityCount = element.Principal.AllIdentityAttributes.Count();

            if (propertyCount != identityCount)
            {
               errorMessages.Add($"{tag} foreign key must have zero or {identityCount} {(identityCount == 1 ? "property" : "properties")} defined, "
                               + $"since {element.Principal.Name} has {identityCount} identity properties. Found {propertyCount} instead");
            }

            // validate names
            foreach (string propertyName in foreignKeyPropertyNames)
            {
               if (!CodeGenerator.IsValidLanguageIndependentIdentifier(propertyName))
                  errorMessages.Add($"{tag} FK property name '{propertyName}' isn't a valid .NET identifier");

               if (element.Dependent.AllAttributes.Except(element.Dependent.Attributes).Any(a => a.Name == propertyName))
                  errorMessages.Add($"{tag} FK property name '{propertyName}' is used in a base class of {element.Dependent.Name}");
            }

            // ensure no FKs are autogenerated identities
            errorMessages.AddRange(element.GetFKAutoIdentityErrors()
                                          .Select(attribute => $"{attribute.Name} in {element.Dependent.FullName} is an auto-generated identity. Migration will fail."));
         }
      }
   }
}