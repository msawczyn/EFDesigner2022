using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Design;
using System;
using System.ComponentModel;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   ///    Encapsulates values of the current Output Location settings in the ModelRoot
   /// </summary>
   [Serializable]
   public class TemporalTableProperties : IHasStore
   {
      private readonly ModelClass modelClass;

      /// <summary>
      ///    Constructor
      /// </summary>
      /// <param name="modelClass">DomainClass ModelClass</param>
      public TemporalTableProperties(ModelClass modelClass)
      {
         this.modelClass = modelClass;
      }

      /// <summary>
      /// Mirrors the ModelClass's UseTemporalTables property.
      /// Used to enable/disable the Temporal Table properties in the property editor.
      /// </summary>
      [DisplayNameResource("Sawczyn.EFDesigner.EFModel.ModelClass/IsTemporalTableActive.DisplayName", typeof(EFModelDomainModel), "Sawczyn.EFDesigner.EFModel.GeneratedCode.DomainModelResx")]
      [CategoryResource("Sawczyn.EFDesigner.EFModel.ModelClass/IsTemporalTableActive.Category", typeof(EFModelDomainModel), "Sawczyn.EFDesigner.EFModel.GeneratedCode.DomainModelResx")]
      [DescriptionResource("Sawczyn.EFDesigner.EFModel.ModelClass/IsTemporalTableActive.Description", typeof(EFModelDomainModel), "Sawczyn.EFDesigner.EFModel.GeneratedCode.DomainModelResx")]
      public bool IsTemporalTableActive
      {
         get
         {
            return modelClass.UseTemporalTables;
         }
         set
         {
            using (Transaction t = modelClass.Store.TransactionManager.BeginTransaction())
            {
               if (value != modelClass.UseTemporalTables)
                  modelClass.UseTemporalTables = value;
               t.Commit();
            }
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
            return modelClass.HistoryTableName;
         }
         set
         {
            using (Transaction t = modelClass.Store.TransactionManager.BeginTransaction())
            {
               if (value != modelClass.HistoryTableName)
                  modelClass.HistoryTableName = value;
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
            return modelClass.PeriodStartColumnName;
         }
         set
         {
            using (Transaction t = modelClass.Store.TransactionManager.BeginTransaction())
            {
               if (value != modelClass.PeriodStartColumnName)
                  modelClass.PeriodStartColumnName = value;
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
            return modelClass.PeriodEndColumnName;
         }
         set
         {
            using (Transaction t = modelClass.Store.TransactionManager.BeginTransaction())
            {
               if (value != modelClass.PeriodEndColumnName)
                  modelClass.PeriodEndColumnName = value;
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