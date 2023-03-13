using System.CodeDom.Compiler;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.Modeling;

using Sawczyn.EFDesigner.EFModel.Extensions;

namespace Sawczyn.EFDesigner.EFModel
{
   [RuleOn(typeof(ModelEnumValue), FireTime = TimeToFire.TopLevelCommit)]
   internal class ModelEnumValueChangeRules : ChangeRule
   {
      public override void ElementPropertyChanged(ElementPropertyChangedEventArgs e)
      {
         base.ElementPropertyChanged(e);

         ModelEnumValue element = (ModelEnumValue)e.ModelElement;

         if (element.IsDeleted)
            return;

         ModelEnum modelEnum = element.Enum;
         ModelRoot modelRoot = modelEnum.ModelRoot;

         Store store = element.Store;
         Transaction current = store.TransactionManager.CurrentTransaction;

         if (current.IsSerializing || ModelRoot.BatchUpdating)
            return;

         if (Equals(e.NewValue, e.OldValue))
            return;

         string errorMessage = null;

         switch (e.DomainProperty.Name)
         {
            case "Name":
               Match match = Regex.Match(modelEnum.Name, @"(.+)\s*=\s*(\d+)");

               if (match != Match.Empty)
               {
                  modelEnum.Name = match.Groups[1].Value;
                  element.Value = match.Groups[2].Value;
               }

               if (string.IsNullOrWhiteSpace(modelEnum.Name) || !CodeGenerator.IsValidLanguageIndependentIdentifier(modelEnum.Name))
                  errorMessage = $"{modelEnum.Name}.{modelEnum.Name}: Name must be a valid .NET identifier";
               else if (modelEnum.Values.Except(new[] {element}).Any(v => v.Name == modelEnum.Name))
                  errorMessage = $"{modelEnum.Name}.{modelEnum.Name}: Name already in use";
               else
               {
                  if (modelRoot.ReservedWords.Contains(element.Name))
                     element.Name = "@" + element.Name;

                  // find ModelAttributes where the default value is this ModelEnumValue and change it to the new name
                  foreach (ModelAttribute modelAttribute in store.GetAll<ModelAttribute>().Where(a => a.InitialValue == $"{modelEnum.Name}.{(string)e.OldValue}"))
                  {
                     string[] parts = modelAttribute.InitialValue.Split('.');
                     parts[1] = modelEnum.Name;
                     modelAttribute.InitialValue = string.Join(".", parts);
                  }
               }

               break;

            case "Value":
               string newValue = (string)e.NewValue;

               if (newValue != null)
               {
                  bool badValue = false;

                  switch (modelEnum.ValueType)
                  {
                     case EnumValueType.Int16:
                        badValue = !short.TryParse(newValue, out short _);

                        break;

                     case EnumValueType.Int32:
                        badValue = !int.TryParse(newValue, out int _);

                        break;

                     case EnumValueType.Int64:
                        badValue = !long.TryParse(newValue, out long _);

                        break;
                  }

                  if (badValue)
                     errorMessage = $"Invalid value for {modelEnum.Name}. Must be {modelEnum.ValueType}.";
                  else
                  {
                     bool hasDuplicates = modelEnum.Values.Any(x => (x != element) && (x.Value == newValue));

                     if (hasDuplicates)
                        errorMessage = $"Value {newValue} is already present in {modelEnum.Name}. Can't have duplicate values.";
                  }
               }

               break;
         }

         if (errorMessage != null)
         {
            current.Rollback();
            ErrorDisplay.Show(store, errorMessage);
         }
      }
   }
}