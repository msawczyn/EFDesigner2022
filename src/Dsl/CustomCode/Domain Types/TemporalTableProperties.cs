using System;
using System.ComponentModel;
using System.Diagnostics;

using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Design;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   ///    Encapsulates values of the current Temporal Table Options settings in the ModelClass
   /// </summary>
   [Serializable]
   public class TemporalTableProperties : IHasStore
   {
      /// <summary>
      /// Initial value for the PeriodEndColumnName property
      /// </summary>
      protected readonly string _defaultPeriodEndColumnName = "PeriodEnd";

      /// <summary>
      /// Initial value for the PeriodStartColumnName property
      /// </summary>
      protected readonly string _defaultPeriodStartColumnName = "PeriodStart";

      /// <summary>
      ///   DomainClass ModelClass
      /// </summary>
      protected ModelClass modelClass;

      /// <summary>
      ///    Constructor
      /// </summary>
      /// <param name="modelClass">DomainClass ModelClass</param>
      public TemporalTableProperties(ModelClass modelClass)
      {
         this.modelClass = modelClass;
      }

      /// <summary>
      /// Initial value for the HistoryTableName property
      /// </summary>
      protected string _defaultHistoryTableName
      {
         get
         {
            return $"{modelClass.Name}History";
         }
      }

      /// <summary>
      ///    Output location value for the generated DbContext-derived object
      /// </summary>

      // ReSharper disable once UnusedMember.Global
      [DisplayNameResource("Sawczyn.EFDesigner.EFModel.ModelClass/HistoryTableName.DisplayName", typeof(EFModelDomainModel), "Sawczyn.EFDesigner.EFModel.GeneratedCode.DomainModelResx")]
      [CategoryResource("Sawczyn.EFDesigner.EFModel.ModelClass/HistoryTableName.Category", typeof(EFModelDomainModel), "Sawczyn.EFDesigner.EFModel.GeneratedCode.DomainModelResx")]
      [DescriptionResource("Sawczyn.EFDesigner.EFModel.ModelClass/HistoryTableName.Description", typeof(EFModelDomainModel), "Sawczyn.EFDesigner.EFModel.GeneratedCode.DomainModelResx")]
      public string HistoryTableName
      {
         get
         {
            string result = string.IsNullOrWhiteSpace(modelClass.HistoryTableName)
                                                   ? _defaultHistoryTableName
                                                   : modelClass.HistoryTableName;
            return result;
         }
         set
         {
            using (Transaction t = modelClass.Store.TransactionManager.BeginTransaction())
            {
               string temp = string.IsNullOrWhiteSpace(value) || (value == _defaultHistoryTableName)
                                ? null
                                : value;

               if (temp != modelClass.HistoryTableName)
               {
                  modelClass.HistoryTableName = temp;
               }

               t.Commit();
            }
         }
      }

      /// <summary>
      ///    Output location value for the generated entity classes
      /// </summary>
      [DisplayNameResource("Sawczyn.EFDesigner.EFModel.ModelClass/PeriodStartColumnName.DisplayName", typeof(EFModelDomainModel), "Sawczyn.EFDesigner.EFModel.GeneratedCode.DomainModelResx")]
      [CategoryResource("Sawczyn.EFDesigner.EFModel.ModelClass/PeriodStartColumnName.Category", typeof(EFModelDomainModel), "Sawczyn.EFDesigner.EFModel.GeneratedCode.DomainModelResx")]
      [DescriptionResource("Sawczyn.EFDesigner.EFModel.ModelClass/PeriodStartColumnName.Description", typeof(EFModelDomainModel), "Sawczyn.EFDesigner.EFModel.GeneratedCode.DomainModelResx")]
      public string PeriodStartColumnName
      {
         get
         {
            string result = string.IsNullOrWhiteSpace(modelClass.PeriodStartColumnName)
                                                        ? _defaultPeriodStartColumnName
                                                        : modelClass.PeriodStartColumnName;
            return result;
         }
         set
         {
            using (Transaction t = modelClass.Store.TransactionManager.BeginTransaction())
            {
               string temp = string.IsNullOrWhiteSpace(value) || value == _defaultPeriodStartColumnName
                                ? null
                                : value;

               if (temp != modelClass.PeriodStartColumnName)
               {
                  modelClass.PeriodStartColumnName = temp;
               }

               t.Commit();
            }
         }
      }

      /// <summary>
      ///    Output location value for the generated enumerations
      /// </summary>
      [DisplayNameResource("Sawczyn.EFDesigner.EFModel.ModelClass/PeriodEndColumnName.DisplayName", typeof(EFModelDomainModel), "Sawczyn.EFDesigner.EFModel.GeneratedCode.DomainModelResx")]
      [CategoryResource("Sawczyn.EFDesigner.EFModel.ModelClass/PeriodEndColumnName.Category", typeof(EFModelDomainModel), "Sawczyn.EFDesigner.EFModel.GeneratedCode.DomainModelResx")]
      [DescriptionResource("Sawczyn.EFDesigner.EFModel.ModelClass/PeriodEndColumnName.Description", typeof(EFModelDomainModel), "Sawczyn.EFDesigner.EFModel.GeneratedCode.DomainModelResx")]
      public string PeriodEndColumnName
      {
         get
         {
            string result = string.IsNullOrWhiteSpace(modelClass.PeriodEndColumnName)
                                                      ? _defaultPeriodEndColumnName
                                                      : modelClass.PeriodEndColumnName;
            return result;
         }
         set
         {
            using (Transaction t = modelClass.Store.TransactionManager.BeginTransaction())
            {
               string temp = string.IsNullOrWhiteSpace(value) || value == _defaultPeriodEndColumnName
                                ? null
                                : value;

               if (temp != modelClass.PeriodEndColumnName)
               {
                  modelClass.PeriodEndColumnName = temp;
               }

               t.Commit();
            }
         }
      }

      /// <summary>
      ///    Exposes the Store object.  Store is a complete model.  Stores contain both the domain data  and the model data for all the domain models in a model.
      /// </summary>
      [Browsable(false)]
      public Store Store
      {
         get
         {
            return modelClass?.Store;
         }
      }

      /// <summary>Returns a string that represents the current object.</summary>
      /// <returns>A string that represents the current object.</returns>
      public override string ToString()
      {
         // to prevent unwanted text in the property editor
         return string.Empty;
      }
   }
}