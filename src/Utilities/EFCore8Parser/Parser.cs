using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using ParsingModels;
// ReSharper disable NotResolvedInText

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

            List<Type> types = assembly.GetExportedTypes().Where(t => typeof(DbContext).IsAssignableFrom(t)).ToList();

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

         Type optionsBuilderType = typeof(DbContextOptionsBuilder<>).MakeGenericType(contextType);
         DbContextOptionsBuilder optionsBuilder = Activator.CreateInstance(optionsBuilderType) as DbContextOptionsBuilder;
         Type optionsType = typeof(DbContextOptions<>).MakeGenericType(contextType);

         DbContextOptions options = optionsBuilder.UseLazyLoadingProxies()
                                                  .UseInMemoryDatabase("Parser")
                                                  .UseInternalServiceProvider(new ServiceCollection().AddEntityFrameworkInMemoryDatabase()
                                                                                                     .AddEntityFrameworkProxies()
                                                                                                     .BuildServiceProvider())
                                                  .Options;

         ConstructorInfo constructor = contextType.GetConstructor(new[] { optionsType });

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
            throw new ArgumentNullException("dbContext");

         model = dbContext.Model;

         ModelRoot modelRoot = ProcessRoot();

         List<ModelClass> modelClasses = model.GetEntityTypes()
                                              .Where(x => !x.ClrType.IsGenericType)
                                              .Select(type => ProcessEntity(type, modelRoot))
                                              .Where(x => x != null)
                                              .ToList();

         modelRoot.Classes.AddRange(modelClasses);

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

         if (type.BaseType?.IsGenericType == true)
         {
            PropertyInfo[] baseProperties = type.BaseType.GetProperties().ToArray();

            foreach (PropertyInfo property in baseProperties)
            {
               result.Properties.Add(ProcessProperty(property, modelRoot));
            }
         }

         result.UnidirectionalAssociations = GetUnidirectionalAssociations(entityType);
         result.BidirectionalAssociations = GetBidirectionalAssociations(entityType);

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
            result.IsFlags = enumType.GetTypeInfo().GetCustomAttribute(typeof(FlagsAttribute)) is FlagsAttribute;
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

       protected ModelProperty ProcessProperty(PropertyInfo propertyData, ModelRoot modelRoot)
      {
         Type type = propertyData.PropertyType;
         List<CustomAttributeData> attributes = propertyData.CustomAttributes.ToList();
      
         ModelProperty result = new ModelProperty();
      
         if (type.IsEnum)
            ProcessEnum(propertyData.PropertyType, modelRoot);
      
         // If it is NULLABLE, then get the underlying type. eg if "Nullable<int>" then this will return just "int"
         bool isNullableType = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

         if (isNullableType)
            type = type.GetGenericArguments()[0];

         result.TypeName = type.IsEnum
                              ? type.FullName
                              : type.Name;
         result.Name = propertyData.Name;

         ColumnAttribute columnAttribute = propertyData.CustomAttributes.OfType<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>().FirstOrDefault();

         if (columnAttribute != null)
         {
            result.ColumnName = columnAttribute.Name;
            attributes.RemoveAll(a => a.AttributeType.Name == "ColumnAttribute");
         }

         result.IsIdentity = propertyData.CustomAttributes.OfType<System.ComponentModel.DataAnnotations.KeyAttribute>().Any();
         //result.IsIdentityGenerated = result.IsIdentity && (propertyData.ValueGenerated == ValueGenerated.OnAdd);

         CustomAttributeData requiredAttribute = attributes.FirstOrDefault(a => a.AttributeType.Name == "RequiredAttribute");

         result.Required = (bool)(requiredAttribute?.ConstructorArguments.FirstOrDefault().Value
                               ?? !isNullableType);
         attributes.RemoveAll(a => a.AttributeType.Name == "RequiredAttribute");

         //result.Indexed = propertyData.IsIndex();
         //result.IndexedUnique = result.Indexed && propertyData.IsUniqueIndex();
         //result.IndexName = propertyData.GetContainingIndexes().FirstOrDefault(i => i.Properties.Count == 1)?.Name;
         //result.MaxStringLength = type == typeof(string) ? (propertyData.GetMaxLength() ?? 0) : 0;

         if (result.MaxStringLength == 0)
            result.MaxStringLength = ParseVarcharColumnTypeAttribute(attributes);

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

     protected ModelProperty ProcessProperty(IProperty propertyData, ModelRoot modelRoot)
      {
         Type type = propertyData.ClrType;

         List<CustomAttributeData> attributes = propertyData.PropertyInfo.CustomAttributes.ToList();

         ModelProperty result = new ModelProperty();

         if (type.IsEnum)
            ProcessEnum(propertyData.ClrType, modelRoot);

         // If it is NULLABLE, then get the underlying type. eg if "Nullable<int>" then this will return just "int"
         if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)))
            type = type.GetGenericArguments()[0];

         result.TypeName = type.IsEnum
                              ? type.FullName
                              : type.Name;

         result.Name = propertyData.Name;
#pragma warning disable CS0618
         result.ColumnName = propertyData.GetColumnName();
#pragma warning restore CS0618
         result.IsIdentity = propertyData.IsKey();
         result.IsIdentityGenerated = result.IsIdentity && (propertyData.ValueGenerated == ValueGenerated.OnAdd);

         CustomAttributeData requiredAttribute = attributes.FirstOrDefault(a => a.AttributeType.Name == "RequiredAttribute");
         result.Required = (bool)(requiredAttribute?.ConstructorArguments.FirstOrDefault().Value ?? !propertyData.IsNullable);
         attributes.RemoveAll(a => a.AttributeType.Name == "RequiredAttribute");

         result.Indexed = propertyData.IsIndex();
         result.IndexedUnique = result.Indexed && propertyData.IsUniqueIndex();
         result.IndexName = propertyData.GetContainingIndexes().FirstOrDefault(i => i.Properties.Count == 1)?.Name;
         result.MaxStringLength = type == typeof(string) ? (propertyData.GetMaxLength() ?? 0) : 0;

         if (result.MaxStringLength == 0)
            result.MaxStringLength = ParseVarcharColumnTypeAttribute(attributes);

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

            List<string> fkPropertyDeclarations = navigationProperty.ForeignKey.Properties
                                                                       .Where(p => !p.IsShadowProperty())
                                                                       .Select(p => p.Name)
                                                                       .ToList();

            association.ForeignKey = fkPropertyDeclarations.Any()
                                        ? string.Join(",", fkPropertyDeclarations)
                                        : null;

            association.SourceRole = navigationProperty.IsOnDependent ? AssociationRole.Dependent : AssociationRole.Principal;
            association.TargetRole = navigationProperty.IsOnDependent ? AssociationRole.Principal : AssociationRole.Dependent;

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

         GetDirectBidirectionalAssociations(entityType, result);
         GetSkipBidirectionalAssociations(entityType, result);

         return result;
      }

      private static void GetSkipBidirectionalAssociations(IEntityType entityType, List<ModelBidirectionalAssociation> result)
      {
         foreach (ISkipNavigation navigationProperty in entityType.GetDeclaredSkipNavigations())
         {
            ModelBidirectionalAssociation association = new ModelBidirectionalAssociation();

            Type sourceType = entityType.ClrType.Unwrap();
            association.SourceClassName = sourceType.Name;
            association.SourceClassNamespace = sourceType.Namespace;

            Type targetType = navigationProperty.TargetEntityType.ClrType.Unwrap();
            association.TargetClassName = targetType.Name;
            association.TargetClassNamespace = targetType.Namespace;

            ISkipNavigation inverse = navigationProperty.Inverse;

            // the property in the source class (referencing the target class)
            association.TargetPropertyTypeName = navigationProperty.PropertyInfo.PropertyType.Unwrap().Name;
            association.TargetPropertyName = navigationProperty.Name;
            association.TargetMultiplicity = Multiplicity.ZeroMany;

            //association.TargetSummary = navigationProperty.ToEndMember.Documentation?.Summary;
            //association.TargetDescription = navigationProperty.ToEndMember.Documentation?.LongDescription;

            // the property in the target class (referencing the source class)
            association.SourcePropertyTypeName = inverse.PropertyInfo.PropertyType.Unwrap().Name;
            association.SourcePropertyName = inverse.Name;
            association.SourceMultiplicity = Multiplicity.ZeroMany;

            //association.SourceSummary = navigationProperty.FromEndMember.Documentation?.Summary;
            //association.SourceDescription = navigationProperty.FromEndMember.Documentation?.LongDescription;

            List<string> fkPropertyDeclarations = navigationProperty.ForeignKey.Properties
                                                                    .Where(p => !p.IsShadowProperty())
                                                                    .Select(p => p.Name)
                                                                    .ToList();

            association.ForeignKey = fkPropertyDeclarations.Any()
                                        ? string.Join(",", fkPropertyDeclarations)
                                        : null;

            association.SourceRole = AssociationRole.NotApplicable;
            association.TargetRole = AssociationRole.NotApplicable;

            association.JoinTableName = navigationProperty.JoinEntityType.GetTableName();

            result.Add(association);
         }
      }

      private static void GetDirectBidirectionalAssociations(IEntityType entityType, List<ModelBidirectionalAssociation> result)
      {
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

            List<string> fkPropertyDeclarations = navigationProperty.ForeignKey.Properties
                                                                    .Where(p => !p.IsShadowProperty())
                                                                    .Select(p => p.Name)
                                                                    .ToList();

            association.ForeignKey = fkPropertyDeclarations.Any()
                                        ? string.Join(",", fkPropertyDeclarations)
                                        : null;

            association.SourceRole = navigationProperty.IsOnDependent ? AssociationRole.Dependent : AssociationRole.Principal;
            association.TargetRole = navigationProperty.IsOnDependent ? AssociationRole.Principal : AssociationRole.Dependent;

            result.Add(association);
         }
      }

      #endregion
   }
}