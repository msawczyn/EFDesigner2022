using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Design;

namespace Sawczyn.EFDesigner.EFModel
{
   public partial class ModelClassTypeDescriptor
   {
      private DomainDataDirectory storeDomainDataDirectory;

      /// <summary>
      ///    Returns the property descriptors for the described ModelClass domain class, adding tracking property
      ///    descriptor(s).
      /// </summary>
      private PropertyDescriptorCollection GetCustomProperties(Attribute[] attributes)
      {
         // Get the default property descriptors from the base class  
         PropertyDescriptorCollection propertyDescriptors = base.GetProperties(attributes);

         if (ModelElement is ModelClass modelClass)
         {
            storeDomainDataDirectory = modelClass.Store.DomainDataDirectory;
            ModelRoot modelRoot = modelClass.ModelRoot;

            if (modelRoot == null)
               return propertyDescriptors;

            // things unavailable if pre-EFCore5
            if (!modelRoot.IsEFCore5Plus)
            {
               propertyDescriptors.Remove("TableComment");
               propertyDescriptors.Remove("IsPropertyBag");
               propertyDescriptors.Remove("IsQueryType");
               propertyDescriptors.Remove("ExcludeFromMigrations");
               propertyDescriptors.Remove("IsDatabaseView");
               propertyDescriptors.Remove("ViewName");
            }

            // things unavailable if pre-EFCore6
            if (!modelRoot.IsEFCore6Plus)
            {
               propertyDescriptors.Remove("UseTemporalTables");
               propertyDescriptors.Remove("TemporalTableOptions");

               if (modelClass.IsDependentType)
                  propertyDescriptors.Remove("TableName");
            }

            // things unavailable if pre-EFCore7
            if (!modelRoot.IsEFCore7Plus)
            {
               if (!modelRoot.GenerateTableComments)
                  propertyDescriptors.Remove("TableComment");

               if (modelClass.IsPropertyBag)
                  propertyDescriptors.Remove("IsDependentType");

               if ((modelClass.Subclasses.Any() && modelClass.InheritanceStrategy != CodeStrategy.TablePerHierarchy)
                || modelClass.Superclass != null)
               {
                  propertyDescriptors.Remove("UseTemporalTables");
                  propertyDescriptors.Remove("TemporalTableOptions");
               }

               if (modelClass.InheritanceStrategy == CodeStrategy.TablePerHierarchy && modelClass.Superclass != null)
               {
                  propertyDescriptors.Remove("TableName");
               }
            }

            // things unavailable if pre-EFCore8
            if (!modelRoot.IsEFCore8Plus)
            {
            }

            // things unavailable for association classes
            if (modelClass.IsAssociationClass)
            {
               propertyDescriptors.Remove("IsAbstract");
               propertyDescriptors.Remove("IsDependentType");
               propertyDescriptors.Remove("IsPropertyBag");
               propertyDescriptors.Remove("IsQueryType");
               propertyDescriptors.Remove("IsDatabaseView");
               propertyDescriptors.Remove("ViewName");
               propertyDescriptors.Remove("ExcludeFromMigrations");
            }

            // things unavailable based on whether there's a view backing the class
            if (modelClass.IsDatabaseView)
            {
               propertyDescriptors.Remove("DbSetName");
               propertyDescriptors.Remove("IsAssociationClass");
               propertyDescriptors.Remove("IsDependentType");
               propertyDescriptors.Remove("IsQueryType");
               propertyDescriptors.Remove("Persistent");
               propertyDescriptors.Remove("TableComment");
               propertyDescriptors.Remove("TableName");
               propertyDescriptors.Remove("UseTemporalTables");
               propertyDescriptors.Remove("TemporalTableOptions");
            }
            else
               propertyDescriptors.Remove("ViewName");

            // things unavailable for query types
            if (modelClass.IsQueryType)
            {
               propertyDescriptors.Remove("Concurrency");
               propertyDescriptors.Remove("DatabaseSchema");
               propertyDescriptors.Remove("DbSetName");
               propertyDescriptors.Remove("IsAssociationClass");
               propertyDescriptors.Remove("IsDatabaseView");
               propertyDescriptors.Remove("Persistent");
               propertyDescriptors.Remove("TableComment");
               propertyDescriptors.Remove("TableName");
               propertyDescriptors.Remove("UseTemporalTables");
               propertyDescriptors.Remove("TemporalTableOptions");
               propertyDescriptors.Remove("ViewName");
            }

            // things unavailable based on persistent/transient status
            if (!modelClass.Persistent)
            {
               propertyDescriptors.Remove("Concurrency");
               propertyDescriptors.Remove("DbSetName");
               propertyDescriptors.Remove("DescribedAssociationElementId");
               propertyDescriptors.Remove("ExcludeFromMigrations");
               propertyDescriptors.Remove("InheritanceStrategy");
               propertyDescriptors.Remove("IsAssociationClass");
               propertyDescriptors.Remove("IsDatabaseView");
               propertyDescriptors.Remove("IsDependentType");
               propertyDescriptors.Remove("IsPropertyBag");
               propertyDescriptors.Remove("IsQueryType");
               propertyDescriptors.Remove("TableComment");
               propertyDescriptors.Remove("TableName");
               propertyDescriptors.Remove("UseTemporalTables");
               propertyDescriptors.Remove("TemporalTableOptions");
               propertyDescriptors.Remove("ViewName");
            }

            if (!modelClass.UseTemporalTables)
            {
               propertyDescriptors.Remove("TemporalTableOptions");
            }

            //Add the descriptors for the tracking properties 

            propertyDescriptors.Add(new TrackingPropertyDescriptor(modelClass,
                                                                   storeDomainDataDirectory.GetDomainProperty(ModelClass.AutoPropertyDefaultDomainPropertyId),
                                                                   storeDomainDataDirectory.GetDomainProperty(ModelClass.IsAutoPropertyDefaultTrackingDomainPropertyId),
                                                                   new Attribute[]
                                                                   {
                                                                      new DisplayNameAttribute("AutoProperty Default"),
                                                                      new DescriptionAttribute("Overrides default autoproperty default setting"),
                                                                      new CategoryAttribute("Code Generation")
                                                                   }));

            if (modelClass.Persistent)
            {
               propertyDescriptors.Add(new TrackingPropertyDescriptor(modelClass,
                                                                      storeDomainDataDirectory.GetDomainProperty(ModelClass.DatabaseSchemaDomainPropertyId),
                                                                      storeDomainDataDirectory.GetDomainProperty(ModelClass.IsDatabaseSchemaTrackingDomainPropertyId),
                                                                      new Attribute[]
                                                                      {
                                                                         new DisplayNameAttribute("Database Schema"),
                                                                         new DescriptionAttribute("The schema to use for table creation. Overrides default schema for model if present."),
                                                                         new CategoryAttribute("Database")
                                                                      }));
            }

            propertyDescriptors.Add(new TrackingPropertyDescriptor(modelClass,
                                                                   storeDomainDataDirectory.GetDomainProperty(ModelClass.NamespaceDomainPropertyId),
                                                                   storeDomainDataDirectory.GetDomainProperty(ModelClass.IsNamespaceTrackingDomainPropertyId),
                                                                   new Attribute[]
                                                                   {
                                                                      new DisplayNameAttribute("Namespace"),
                                                                      new DescriptionAttribute("Overrides default namespace"),
                                                                      new CategoryAttribute("Code Generation")
                                                                   }));

            propertyDescriptors.Add(new TrackingPropertyDescriptor(modelClass,
                                                                   storeDomainDataDirectory.GetDomainProperty(ModelClass.DefaultConstructorVisibilityDomainPropertyId),
                                                                   storeDomainDataDirectory.GetDomainProperty(ModelClass.IsDefaultConstructorVisibilityTrackingDomainPropertyId),
                                                                   new Attribute[]
                                                                   {
                                                                      new DisplayNameAttribute("Default Constructor Visibility"),
                                                                      new
                                                                         DescriptionAttribute("By default, default (empty) constructors generate as public unless there are required properties or associations in the entity, then they generate as protected."),
                                                                      new CategoryAttribute("Code Generation")
                                                                   }));

            propertyDescriptors.Add(new TrackingPropertyDescriptor(modelClass,
                                                                   storeDomainDataDirectory.GetDomainProperty(ModelClass.OutputDirectoryDomainPropertyId),
                                                                   storeDomainDataDirectory.GetDomainProperty(ModelClass.IsOutputDirectoryTrackingDomainPropertyId),
                                                                   new Attribute[]
                                                                   {
                                                                      new DisplayNameAttribute("Output Directory"),
                                                                      new DescriptionAttribute("Overrides default output directory"),
                                                                      new CategoryAttribute("Code Generation"),
                                                                      new TypeConverterAttribute(typeof(ProjectDirectoryTypeConverter))
                                                                   }));

            if (modelRoot.IsEFCore7Plus && modelClass.Persistent)
            {
               propertyDescriptors.Add(new TrackingPropertyDescriptor(modelClass
                                                                    , storeDomainDataDirectory.GetDomainProperty(ModelClass.InheritanceStrategyDomainPropertyId)
                                                                    , storeDomainDataDirectory.GetDomainProperty(ModelClass.IsInheritanceStrategyTrackingDomainPropertyId)
                                                                    , new Attribute[]
                                                                      {
                                                                         new DisplayNameAttribute("Inheritance Strategy")
                                                                       , new DescriptionAttribute("Overrides default inheritance strategy")
                                                                       , new CategoryAttribute("Code Generation")
                                                                      }));
            }
         }

         // Return the property descriptors for this element  
         return propertyDescriptors;
      }
   }
}