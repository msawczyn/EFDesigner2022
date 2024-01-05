﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using ParsingModels;

using PluralizeService.Core;

// ReSharper disable UseObjectOrCollectionInitializer
#pragma warning disable IDE0017 // Simplify object initialization

namespace EFCore8Parser
{
   public class Parser : ParserBase
   {
      private readonly DbContext dbContext;

      private IModel model;

      public Parser(Assembly assembly, Logger logger, string dbContextTypeName = null) : base(logger)
      {
         Type contextType;

         if (dbContextTypeName != null)
         {
            log.Debug($"dbContextTypeName parameter is {dbContextTypeName}");
            contextType = assembly.GetExportedTypes().FirstOrDefault(t => (t.FullName == dbContextTypeName) || (t.Name == dbContextTypeName));
            log.Info($"Using contextType = {contextType.FullName}");
         }
         else
         {
            log.Debug("dbContextTypeName parameter is null");

            List<Type> types = assembly.GetExportedTypes().Where(t => typeof( DbContext ).IsAssignableFrom(t)).ToList();

            // ReSharper disable once UnthrowableException
            if (types.Count == 0)
            {
               log.Error($"No usable DBContext found in {assembly.FullName}");

               throw new ArgumentException("Couldn't find DbContext-derived class in assembly. Is it public?");
            }

            if (types.Count > 1)
            {
               string msg = $"Found more than one class derived from DbContext: {string.Join(", ", types.Select(t => t.FullName))}";
               log.Error(msg);

               throw new AmbiguousMatchException(msg);
            }

            contextType = types[0];
            log.Info($"Using contextType = {contextType.FullName}");
         }

         Type optionsBuilderType = typeof( DbContextOptionsBuilder<> ).MakeGenericType(contextType);
         DbContextOptionsBuilder optionsBuilder = Activator.CreateInstance(optionsBuilderType) as DbContextOptionsBuilder;
         Type optionsType = typeof( DbContextOptions<> ).MakeGenericType(contextType);

         DbContextOptions options = optionsBuilder.UseLazyLoadingProxies()
                                                  .UseInMemoryDatabase("Parser")
                                                  .UseInternalServiceProvider(new ServiceCollection().AddEntityFrameworkInMemoryDatabase()
                                                                                                     .AddEntityFrameworkProxies()
                                                                                                     .BuildServiceProvider())
                                                  .Options;

         Type[] constructorTypes = { optionsType };
         ConstructorInfo constructor = contextType.GetConstructor(constructorTypes);

         // ReSharper disable once UnthrowableException
         if (constructor != null)
            dbContext = assembly.CreateInstance(contextType.FullName, true, BindingFlags.Default, null, new object[] { options }, null, null) as DbContext;
         else
         {
            constructor = contextType.GetConstructor(Type.EmptyTypes);

            if (constructor != null)
               dbContext = assembly.CreateInstance(contextType.FullName, true, BindingFlags.Default, null, null, null, null) as DbContext;
         }

         if (dbContext == null)
            throw new MissingMethodException($"Can't find appropriate constructor - default or {contextType.Name}.{contextType.Name}(DbContextOptions<{contextType.Name}>)");

         model = dbContext.Model;
      }

      public string Process()
      {
         if (dbContext == null)

            // ReSharper disable once NotResolvedInText
            throw new ArgumentNullException("dbContext");

         model = dbContext.Model;

         ModelRoot modelRoot = ProcessRoot();

         List<ModelClass> modelClasses = model.GetEntityTypes()
                                              .Where(x => !x.ClrType.IsGenericType)
                                              .Select(type => ProcessEntity(type, modelRoot))
                                              .Where(x => x != null)
                                              .ToList();

         modelRoot.Classes.AddRange(modelClasses);
         ProcessShadowClasses(modelRoot);

         return JsonConvert.SerializeObject(modelRoot);
      }

      protected ModelClass ProcessEntity(IEntityType entityType, ModelRoot modelRoot)
      {
         ModelClass result = new ModelClass();
         Type type = entityType.ClrType;

         result.Name = type.Name;
         result.Namespace = type.Namespace;
         result.IsAbstract = type.IsAbstract;

         result.BaseClass = GetTypeFullName(type.BaseType);

         result.ViewName = entityType.GetViewName();
         result.TableName = result.ViewName == null
                               ? entityType.GetTableName()
                               : null;

         result.IsDependentType = entityType.IsOwned();
         result.CustomAttributes = GetCustomAttributes(type.CustomAttributes);

         result.CustomInterfaces = type.GetInterfaces().Any()
                                      ? string.Join(",", type.GetInterfaces().Select(GetTypeFullName).Where(s => s != null))
                                      : null;

         result.Properties = entityType.GetDeclaredProperties()
                                       .Where(p => !p.IsShadowProperty())
                                       .Select(p => ProcessProperty(p, modelRoot))
                                       .Where(x => x != null)
                                       .ToList();

         result.UnidirectionalAssociations = GetUnidirectionalAssociations(entityType);
         result.BidirectionalAssociations = GetBidirectionalAssociations(entityType);

         CheckForShadowClass(result);

         return result;
      }

      protected void ProcessEnum(Type enumType, ModelRoot modelRoot)
      {
         string customAttributes = GetCustomAttributes(enumType);

         ModelEnum result = new ModelEnum();
         result.Name = enumType.Name;
         result.Namespace = enumType.Namespace;

         if (modelRoot.Enumerations.All(e => e.FullName != result.FullName))
         {
            Type underlyingType = Enum.GetUnderlyingType(enumType);
            result.IsFlags = enumType.GetTypeInfo().GetCustomAttribute(typeof( FlagsAttribute )) is FlagsAttribute;
            result.ValueType = underlyingType.Name;

            result.CustomAttributes = customAttributes.Length > 2
                                         ? customAttributes
                                         : null;

            result.Values = Enum.GetNames(enumType)
                                .Select(name => new ModelEnumValue { Name = name, Value = Convert.ChangeType(Enum.Parse(enumType, name), underlyingType).ToString() })
                                .ToList();

            modelRoot.Enumerations.Add(result);
         }
      }

      protected ModelProperty ProcessProperty(IProperty propertyData, ModelRoot modelRoot)
      {
         Type type = propertyData.ClrType;

         List<CustomAttributeData> attributes = propertyData.PropertyInfo.CustomAttributes.ToList();

         ModelProperty result = new ModelProperty();

         if (type.IsEnum)
            ProcessEnum(propertyData.ClrType, modelRoot);

         // If it is NULLABLE, then get the underlying type. eg if "Nullable<int>" then this will return just "int"
         if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof( Nullable<> )))
            type = type.GetGenericArguments()[0];

         result.TypeName = type.IsEnum
                              ? type.FullName
                              : type.Name;

         result.Name = propertyData.Name;
         result.ColumnName = propertyData.GetColumnName();
         result.IsIdentity = propertyData.IsKey();
         result.IsIdentityGenerated = result.IsIdentity && (propertyData.ValueGenerated == ValueGenerated.OnAdd);

         CustomAttributeData requiredAttribute = attributes.FirstOrDefault(a => a.AttributeType.Name == "RequiredAttribute");
         result.Required = (bool)(requiredAttribute?.ConstructorArguments.FirstOrDefault().Value ?? !propertyData.IsNullable);
         attributes.RemoveAll(a => a.AttributeType.Name == "RequiredAttribute");

         result.Indexed = propertyData.IsIndex();
         result.IndexedUnique = result.Indexed && propertyData.IsUniqueIndex();
         result.IndexName = propertyData.GetContainingIndexes().FirstOrDefault(i => i.Properties.Count == 1)?.Name;

         result.MaxStringLength = type == typeof( string )
                                     ? (propertyData.GetMaxLength() ?? 0)
                                     : 0;
         if (result.MaxStringLength == 0)
         {
            Regex varcharPattern = new Regex(@"varacr\((.+)\)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

            List<CustomAttributeData> typeNameAttributes = attributes.Where(a => a.AttributeType.Name == "TypeName").ToList();

            foreach (CustomAttributeData attributeData in typeNameAttributes)
            {
               if (varcharPattern.Match(attributeData.ConstructorArguments))
            }
            result.MaxStringLength = typeNameAttributes.Any()
                                        ? (int)typeNameAttributes.First().ConstructorArguments.First().Value
                                        : 0;
         }

         attributes.RemoveAll(a => (a.AttributeType.Name == "MaxLengthAttribute")
                                || (a.AttributeType.Name == "StringLengthAttribute"));

         CustomAttributeData minLengthAttribute = attributes.FirstOrDefault(a => a.AttributeType.Name == "MinLengthAttribute");
         result.MinStringLength = (int?)minLengthAttribute?.ConstructorArguments.FirstOrDefault().Value ?? 0;
         attributes.RemoveAll(a => a.AttributeType.Name == "MinLengthAttribute");

         string customAttributes = GetCustomAttributes(attributes);

         result.CustomAttributes = customAttributes.Length > 2
                                      ? customAttributes
                                      : null;

         return result;
      }

      protected ModelRoot ProcessRoot()
      {
         ModelRoot result = new ModelRoot();
         Type contextType = dbContext.GetType();

         result.EntityContainerName = contextType.Name;
         result.Namespace = contextType.Namespace;

         return result;
      }

      private void ProcessShadowClasses(ModelRoot modelRoot)
      {
         foreach (PossibleAssociationMerge mergeData in PossibleAssociationMerges)
         {
            ModelClass sourceType = modelRoot.Classes.FirstOrDefault(x => x.Name == mergeData.SourceClassName);
            ModelClass targetType = modelRoot.Classes.FirstOrDefault(x => x.Name == mergeData.TargetClassName);

            if (sourceType != null && targetType != null)
            {
               ModelBidirectionalAssociation association = new ModelBidirectionalAssociation();

               association.SourceClassName = sourceType.Name;
               association.SourceClassNamespace = sourceType.Namespace;
               association.SourceMultiplicity = Multiplicity.ZeroMany;
               association.SourcePropertyName = PluralizationProvider.Pluralize(sourceType.Name);

               association.TargetClassName = targetType.Name;
               association.TargetClassNamespace = targetType.Namespace;
               association.TargetMultiplicity = Multiplicity.ZeroMany;
               association.TargetPropertyName = PluralizationProvider.Pluralize(targetType.Name);

               association.JoinTableName = mergeData.JoinClass.TableName;

               sourceType.BidirectionalAssociations.Remove(mergeData.AssociationToSource);
               sourceType.BidirectionalAssociations.Remove(mergeData.AssociationToTarget);
               sourceType.BidirectionalAssociations.Add(association);

               modelRoot.Classes.Remove(mergeData.JoinClass);
            }
         }
      }

#region Associations

      protected List<ModelUnidirectionalAssociation> GetUnidirectionalAssociations(IEntityType entityType)
      {
         List<ModelUnidirectionalAssociation> result = new List<ModelUnidirectionalAssociation>();

         foreach (INavigation navigationProperty in entityType.GetDeclaredNavigations().Where(n => n.Inverse == null))
         {
            ModelUnidirectionalAssociation association = new ModelUnidirectionalAssociation();

            association.SourceClassName = navigationProperty.DeclaringType.ClrType.Name;
            association.SourceClassNamespace = navigationProperty.DeclaringType.ClrType.Namespace;

            Type targetType = navigationProperty.TargetEntityType.ClrType.Unwrap();
            association.TargetClassName = targetType.Name;
            association.TargetClassNamespace = targetType.Namespace;

            // the property in the source class (referencing the target class)
            association.TargetPropertyTypeName = navigationProperty.PropertyInfo.PropertyType.Unwrap().Name;
            association.TargetPropertyName = navigationProperty.Name;
            association.TargetMultiplicity = ConvertMultiplicity(navigationProperty.GetTargetMultiplicity());

            // the property in the target class (referencing the source class)
            association.SourceMultiplicity = ConvertMultiplicity(navigationProperty.GetSourceMultiplicity());

            if (navigationProperty.ForeignKey != null)
            {
               List<string> fkPropertyDeclarations = navigationProperty.ForeignKey.Properties
                                                                       .Where(p => !p.IsShadowProperty())
                                                                       .Select(p => p.Name)
                                                                       .ToList();

               association.ForeignKey = fkPropertyDeclarations.Any()
                                           ? string.Join(",", fkPropertyDeclarations)
                                           : null;
            }

            // unfortunately, EFCore doesn't serialize documentation like EF6 did

            //association.TargetSummary = navigationProperty.ToEndMember.Documentation?.Summary;
            //association.TargetDescription = navigationProperty.ToEndMember.Documentation?.LongDescription;
            //association.SourceSummary = navigationProperty.FromEndMember.Documentation?.Summary;
            //association.SourceDescription = navigationProperty.FromEndMember.Documentation?.LongDescription;

            result.Add(association);
         }

         return result;
      }

      protected List<ModelBidirectionalAssociation> GetBidirectionalAssociations(IEntityType entityType)
      {
         List<ModelBidirectionalAssociation> result = new List<ModelBidirectionalAssociation>();

         foreach (INavigation navigationProperty in entityType.GetDeclaredNavigations().Where(n => n.Inverse != null))
         {
            ModelBidirectionalAssociation association = new ModelBidirectionalAssociation();

            Type sourceType = navigationProperty.GetSourceType().ClrType.Unwrap();
            association.SourceClassName = sourceType.Name;
            association.SourceClassNamespace = sourceType.Namespace;

            Type targetType = navigationProperty.TargetEntityType.ClrType.Unwrap();
            association.TargetClassName = targetType.Name;
            association.TargetClassNamespace = targetType.Namespace;

            INavigation inverse = navigationProperty.Inverse;

            // the property in the source class (referencing the target class)
            association.TargetPropertyTypeName = navigationProperty.PropertyInfo.PropertyType.Unwrap().Name;
            association.TargetPropertyName = navigationProperty.Name;
            association.TargetMultiplicity = ConvertMultiplicity(navigationProperty.GetTargetMultiplicity());

            //association.TargetSummary = navigationProperty.ToEndMember.Documentation?.Summary;
            //association.TargetDescription = navigationProperty.ToEndMember.Documentation?.LongDescription;

            // the property in the target class (referencing the source class)
            association.SourcePropertyTypeName = inverse.PropertyInfo.PropertyType.Unwrap().Name;
            association.SourcePropertyName = inverse.Name;
            association.SourceMultiplicity = ConvertMultiplicity(navigationProperty.GetSourceMultiplicity());

            //association.SourceSummary = navigationProperty.FromEndMember.Documentation?.Summary;
            //association.SourceDescription = navigationProperty.FromEndMember.Documentation?.LongDescription;

            if (navigationProperty.ForeignKey != null)
            {
               List<string> fkPropertyDeclarations = navigationProperty.ForeignKey.Properties
                                                                       .Where(p => !p.IsShadowProperty())
                                                                       .Select(p => p.Name)
                                                                       .ToList();

               association.ForeignKey = fkPropertyDeclarations.Any()
                                           ? string.Join(",", fkPropertyDeclarations)
                                           : null;
            }

            result.Add(association);
         }

         return result;
      }

      protected void CheckForShadowClass(ModelClass modelClass)
      {
         ModelClass result = modelClass;

         if (result.Properties.Count == 2 && result.Properties.All(p => p.IsIdentity) && result.BidirectionalAssociations.Count == 2)
         {
            PossibleAssociationMerge possibleMerge = new PossibleAssociationMerge();

            possibleMerge.JoinClass = result;
            possibleMerge.AssociationToSource = result.BidirectionalAssociations[0];
            possibleMerge.AssociationToTarget = result.BidirectionalAssociations[1];

            if (possibleMerge.AssociationToSource.SourceClassName == result.Name)
            {
               possibleMerge.SourceClassName = possibleMerge.AssociationToSource.TargetClassName;

               if (possibleMerge.AssociationToSource.TargetMultiplicity != Multiplicity.One || possibleMerge.AssociationToSource.SourceMultiplicity != Multiplicity.ZeroMany)
                  return;
            }
            else
            {
               possibleMerge.SourceClassName = possibleMerge.AssociationToSource.SourceClassName;

               if (possibleMerge.AssociationToSource.SourceMultiplicity != Multiplicity.One || possibleMerge.AssociationToSource.TargetMultiplicity != Multiplicity.ZeroMany)
                  return;
            }

            if (possibleMerge.AssociationToTarget.SourceClassName == result.Name)
            {
               possibleMerge.TargetClassName = possibleMerge.AssociationToTarget.TargetClassName;

               if (possibleMerge.AssociationToTarget.TargetMultiplicity != Multiplicity.One || possibleMerge.AssociationToTarget.SourceMultiplicity != Multiplicity.ZeroMany)
                  return;
            }
            else
            {
               possibleMerge.TargetClassName = possibleMerge.AssociationToTarget.SourceClassName;

               if (possibleMerge.AssociationToTarget.SourceMultiplicity != Multiplicity.One || possibleMerge.AssociationToTarget.TargetMultiplicity != Multiplicity.ZeroMany)
                  return;
            }

            PossibleAssociationMerges.Add(possibleMerge);
         }
      }

      public class PossibleAssociationMerge
      {
         public ModelClass JoinClass { get; set; }
         public string SourceClassName { get; set; }
         public ModelClass SourceClass { get; set; }
         public string TargetClassName { get; set; }
         public ModelClass TargetClass { get; set; }
         public ModelBidirectionalAssociation AssociationToSource { get; set; }
         public ModelBidirectionalAssociation AssociationToTarget { get; set; }
      }

      protected readonly List<PossibleAssociationMerge> PossibleAssociationMerges = new List<PossibleAssociationMerge>();

#endregion
   }
}