using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

// ReSharper disable RedundantNameQualifier
// ReSharper disable UnusedMember.Global

namespace Sawczyn.EFDesigner.EFModel.EditingOnly
{
   /// <summary>
   /// Represents a text transformation.
   /// </summary>
   public partial class GeneratedTextTransformation
   {
      #region Template

      // EFDesigner v4.2.8.0
      // Copyright (c) 2017-2023 Michael Sawczyn
      // https://github.com/msawczyn/EFDesigner

      /// <summary>
      /// Abstract class representing the EF Core model generator.
      /// </summary>
      public abstract class EFCoreModelGenerator : EFModelGenerator

      {
         /// <summary>
         /// Protected constructor for the EFCoreModelGenerator class.
         /// </summary>
         /// <param name="host">The GeneratedTextTransformation instance.</param>
         protected EFCoreModelGenerator(GeneratedTextTransformation host) : base(host) { }

         /// <summary>
         /// Gets the list of spatial types supported by the database provider.
         /// </summary>
         public static string[] SpatialTypes
         {
            get
            {
               return new[] { "Geometry", "GeometryPoint", "GeometryLineString", "GeometryPolygon", "GeometryCollection", "GeometryMultiPoint", "GeometryMultiLineString", "GeometryMultiPolygon" };
            }
         }

         /// <summary>
         /// Configures bidirectional associations for a given ModelClass object.
         /// </summary>
         /// <param name="modelClass">The ModelClass object for which bidirectional associations need to be configured.</param>
         /// <param name="visited">A list of visited Associations to avoid infinite recursion.</param>
         /// <param name="foreignKeyColumns">A list of foreign key columns.</param>
         /// <param name="declaredShadowProperties">A list of shadow properties declared for the current ModelClass object.</param>
         [SuppressMessage("ReSharper", "RedundantNameQualifier")]
         protected virtual void ConfigureBidirectionalAssociations(ModelClass modelClass, List<Association> visited, List<string> foreignKeyColumns, List<string> declaredShadowProperties)
         {
            WriteBidirectionalNonDependentAssociations(modelClass, visited, foreignKeyColumns);
            WriteBidirectionalDependentAssociations(modelClass, $"modelBuilder.Entity<{modelClass.FullName}>()", visited);
         }

         /// <summary>
         /// Creates the code segments configuring the attributes of a model class.
         /// </summary>
         /// <param name="segments">Container holding the new code segments.</param>
         /// <param name="modelClass">The model class to be configured.</param>
         protected virtual void ConfigureModelAttributes(List<string> segments, ModelClass modelClass)
         {
            foreach (ModelAttribute modelAttribute in modelClass.Attributes.Where(x => x.Persistent && !SpatialTypes.Contains(x.Type)))
            {
               segments.Clear();

               segments.AddRange(GatherModelAttributeSegments(modelAttribute));

               if (segments.Any())
               {
                  segments.Insert(0, $"modelBuilder.Entity<{modelClass.FullName}>()");
                  segments.Insert(1, $"Property(t => t.{modelAttribute.Name})");

                  Output(segments);
               }

               if (modelAttribute.Indexed && !modelAttribute.IsIdentity)
               {
                  segments.Clear();

                  segments.Add(!string.IsNullOrEmpty(modelAttribute.IndexName)
                                  ? $"modelBuilder.Entity<{modelClass.FullName}>().HasIndex(t => t.{modelAttribute.Name}, {modelAttribute.IndexName})"
                                  : $"modelBuilder.Entity<{modelClass.FullName}>().HasIndex(t => t.{modelAttribute.Name})");

                  if (modelAttribute.IndexedUnique)
                     segments.Add("IsUnique()");

                  Output(segments);
               }
            }
         }

         /// <summary>
         /// Creates the code segments configuring a model class.
         /// </summary>
         /// <param name="segments">Container holding the new code segments.</param>
         /// <param name="classesWithTables">The array of classes with tables.</param>
         /// <param name="foreignKeyColumns">The list of foreign key columns.</param>
         /// <param name="visited">The list of already-visited associations.</param>
         /// <param name="modelClass">The model class to be configured.</param>
         protected virtual void ConfigureModelClass(List<string> segments, ModelClass[] classesWithTables, List<string> foreignKeyColumns, List<Association> visited, ModelClass modelClass)
         {
            segments.Clear();
            foreignKeyColumns.Clear();
            NL();

            if (modelClass.IsDependentType)
            {
               segments.Add($"modelBuilder.Owned<{modelClass.FullName}>()");
               Output(segments);

               return;
            }

            segments.Add($"modelBuilder.Entity<{modelClass.FullName}>()");

            ConfigureTransientProperties(segments, modelClass);

            if (classesWithTables.Contains(modelClass))
            {
               if (modelClass.IsQueryType)
               {
                  Output($"// There is no storage defined for {modelClass.Name} because its IsQueryType value is");
                  Output($"// set to 'true'. Please provide the {modelRoot.FullName}.Get{modelClass.Name}SqlQuery() method in the partial class.");
                  Output("// ");
                  Output($"// private string Get{modelClass.Name}SqlQuery()");
                  Output("// {");
                  Output($"//    return the defining SQL query that pulls all the properties for {modelClass.FullName}");
                  Output("// }");

                  segments.Add($"ToSqlQuery(Get{modelClass.Name}SqlQuery())");
               }
               else
                  ConfigureTable(segments, modelClass);
            }

            if ((segments.Count > 1) || modelClass.IsDependentType)
               Output(segments);

            // attribute level
            ConfigureModelAttributes(segments, modelClass);

            bool hasDefinedConcurrencyToken = modelClass.AllAttributes.Any(x => x.IsConcurrencyToken);

            if (!hasDefinedConcurrencyToken && (modelClass.EffectiveConcurrency == ConcurrencyOverride.Optimistic))
               Output($@"modelBuilder.Entity<{modelClass.FullName}>().Property<byte[]>(""Timestamp"").IsConcurrencyToken();");

            // Navigation endpoints are distingished as Source and Target. They are also distinguished as Principal
            // and Dependent. So how do these map to each other? Short answer: they don't - they're orthogonal concepts.
            // Source and Target are accidents of where the user started drawing the association, and help define where the
            // properties are in unidirectional associations. Principal and Dependent define where the foreign keys go in 
            // the persistence mechanism.

            // What matters to code generation is the Principal and Dependent classifications, so we focus on those. 
            // In the case of 1-1 or 0/1-0/1, it's situational, so the user has to tell us.
            // In all other cases, we can tell by the cardinalities of the associations.

            // navigation properties
            List<string> declaredShadowProperties = new List<string>();

            if (!modelClass.IsDependentType)
            {
               ConfigureUnidirectionalAssociations(modelClass, visited, foreignKeyColumns, declaredShadowProperties);
               ConfigureBidirectionalAssociations(modelClass, visited, foreignKeyColumns, declaredShadowProperties);
            }
         }

         /// <summary>
         /// Creates the code segments configuring a collection of model classes.
         /// </summary>
         /// <param name="segments">Container holding the new code segments.</param>
         /// <param name="classesWithTables">The array of model classes with tables.</param>
         /// <param name="foreignKeyColumns">The list of foreign key columns.</param>
         /// <param name="visited">The list of already-visited associations.</param>
         protected virtual void ConfigureModelClasses(List<string> segments, ModelClass[] classesWithTables, List<string> foreignKeyColumns, List<Association> visited)
         {
            foreach (ModelClass modelClass in modelRoot.Classes.Where(x => x.Persistent).OrderBy(x => x.Name))
               ConfigureModelClass(segments, classesWithTables, foreignKeyColumns, visited, modelClass);
         }

         /// <summary>
         /// Creates the code segments configuring the database table backing a model class.
         /// </summary>
         /// <param name="segments">Container holding the new code segments.</param>
         /// <param name="modelClass">The model class to base the table configuration on.</param>
         protected virtual void ConfigureTable(List<string> segments, ModelClass modelClass)
         {
            string tableName = string.IsNullOrEmpty(modelClass.TableName)
                                  ? modelClass.Name
                                  : modelClass.TableName;

            string schema = string.IsNullOrEmpty(modelClass.DatabaseSchema) || (modelClass.DatabaseSchema == modelClass.ModelRoot.DatabaseSchema)
                               ? string.Empty
                               : $", \"{modelClass.DatabaseSchema}\"";

            segments.Add($"ToTable(\"{tableName}\"{schema})");

            // primary key code segments must be output last, since HasKey returns a different type
            List<ModelAttribute> identityAttributes = modelClass.IdentityAttributes.ToList();

            if (identityAttributes.Count == 1)
               segments.Add($"HasKey(t => t.{identityAttributes[0].Name})");
            else if (identityAttributes.Count > 1)
               segments.Add($"HasKey(t => new {{ t.{string.Join(", t.", identityAttributes.Select(ia => ia.Name))} }})");
         }

         /// <summary>
         /// Creates the code segments configuring the transient (non-persistent) attributes of a model class.
         /// </summary>
         /// <param name="segments">Container holding the new code segments.</param>
         /// <param name="modelClass">The ModelClass object to be configured.</param>
         protected static void ConfigureTransientProperties(List<string> segments, ModelClass modelClass)
         {
            foreach (ModelAttribute transient in modelClass.Attributes.Where(x => !x.Persistent))
               segments.Add($"Ignore(t => t.{transient.Name})");
         }

         /// <summary>
         /// Configures and writes the unidirectional associations of a model class.
         /// </summary>
         /// <param name="modelClass">The model class to configure.</param>
         /// <param name="visited">A list of already visited associations.</param>
         /// <param name="foreignKeyColumns">A list of foreign key columns.</param>
         /// <param name="declaredShadowProperties">A list of declared shadow properties.</param>
         [SuppressMessage("ReSharper", "RedundantNameQualifier")]
         protected virtual void ConfigureUnidirectionalAssociations(ModelClass modelClass, List<Association> visited, List<string> foreignKeyColumns, List<string> declaredShadowProperties)
         {
            WriteUnidirectionalNonDependentAssociations(modelClass, visited, foreignKeyColumns);
            WriteUnidirectionalDependentAssociations(modelClass, $"modelBuilder.Entity<{modelClass.FullName}>()", visited);
         }

         /// <summary>
         /// Creates a string representing a foreign key configuration for the given association and foreign key column(s).
         /// </summary>
         /// <param name="association">The association for which the foreign key segment should be created.</param>
         /// <param name="foreignKeyColumns">The foreign key columns to be included in the segment.</param>
         /// <returns>A string representing the foreign key segment of an SQL statement.</returns>
         [SuppressMessage("ReSharper", "RedundantNameQualifier")]
         protected virtual string CreateForeignKeySegment(Association association, List<string> foreignKeyColumns)
         {
            List<string> foreignKeys = GetForeignKeys(association, foreignKeyColumns).ToList();

            if (!foreignKeys.Any()) // only happens if many-to-many
               return null;

            // 1-1, 1-0/1 and 0/1-0/1  
            bool dependentClassDesignationRequired = (association.SourceMultiplicity != Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroMany)
                                                  && (association.TargetMultiplicity != Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroMany);

            string result = string.Join(",", foreignKeys);

            if (foreignKeys.First().StartsWith("\""))
            {
               // foreign keys are shadow properties
               result = dependentClassDesignationRequired
                           ? $"HasForeignKey(\"{association.Dependent.Name}\", {result})"
                           : $"HasForeignKey({result})";
            }
            else
            {
               // foreign keys are real properties
               result = foreignKeys.Count == 1
                           ? $"k => {result}"
                           : $"k => new {{ {result} }}";

               result = dependentClassDesignationRequired
                           ? $"HasForeignKey<{association.Dependent.FullName}>({result})"
                           : $"HasForeignKey({result})";
            }

            return result;
         }

         /// <summary>
         /// Creates the code segments of a model attribute.
         /// </summary>
         /// <param name="modelAttribute">The model attribute.</param>
         /// <returns>A list of strings representing the segments of the model attribute.</returns>
         protected virtual List<string> GatherModelAttributeSegments(ModelAttribute modelAttribute)
         {
            List<string> segments = new List<string>();

            if ((modelAttribute.MaxLength != null) && (modelAttribute.MaxLength > 0))
               segments.Add($"HasMaxLength({modelAttribute.MaxLength})");

            if ((modelAttribute.ColumnName != modelAttribute.Name) && !string.IsNullOrEmpty(modelAttribute.ColumnName))
               segments.Add($"HasColumnName(\"{modelAttribute.ColumnName}\")");

            if (!modelAttribute.AutoProperty)
            {
               segments.Add($"HasField(\"{modelAttribute.BackingFieldName}\")");
               segments.Add($"UsePropertyAccessMode(PropertyAccessMode.{modelAttribute.PropertyAccessMode})");
            }

            if (!string.IsNullOrEmpty(modelAttribute.ColumnType) && (modelAttribute.ColumnType.ToLowerInvariant() != "default"))
            {
               if ((modelAttribute.ColumnType.ToLowerInvariant() == "varchar")
                || (modelAttribute.ColumnType.ToLowerInvariant() == "nvarchar")
                || (modelAttribute.ColumnType.ToLowerInvariant() == "char"))
                  segments.Add($"HasColumnType(\"{modelAttribute.ColumnType}({(modelAttribute.MaxLength > 0 ? modelAttribute.MaxLength.ToString() : "max")})\")");
               else
                  segments.Add($"HasColumnType(\"{modelAttribute.ColumnType}\")");
            }

            if (modelAttribute.IsConcurrencyToken)
               segments.Add("IsRowVersion()");

            if (modelAttribute.IsIdentity)
            {
               segments.Add(modelAttribute.IdentityType == IdentityType.AutoGenerated
                               ? "ValueGeneratedOnAdd()"
                               : "ValueGeneratedNever()");
            }

            if (modelAttribute.Required)
               segments.Add("IsRequired()");

            return segments;
         }

         /// <summary>
         /// Generates all necessary files using the specified EF model file manager.
         /// </summary>
         /// <param name="manager">The EF model file manager to use.</param>
         public override void Generate(Manager manager)
         {
            // Entities
            string fileNameMarker = string.IsNullOrEmpty(modelRoot.FileNameMarker)
                                       ? string.Empty
                                       : $".{modelRoot.FileNameMarker}";

            foreach (ModelClass modelClass in modelRoot.Classes.Where(e => e.GenerateCode))
            {
               ClearIndent();
               manager.StartNewFile(Path.Combine(modelClass.EffectiveOutputDirectory, $"{modelClass.Name}{fileNameMarker}.cs"));
               Output($"#nullable {(modelRoot.GenerateNullable ? "enable" : "disable")}");
               WriteClass(modelClass);
            }

            // Enums
            foreach (ModelEnum modelEnum in modelRoot.Enums.Where(e => e.GenerateCode))
            {
               ClearIndent();
               manager.StartNewFile(Path.Combine(modelEnum.EffectiveOutputDirectory, $"{modelEnum.Name}{fileNameMarker}.cs"));
               Output($"#nullable {(modelRoot.GenerateNullable ? "enable" : "disable")}");
               WriteEnum(modelEnum);
            }

            // Context
            ClearIndent();
            manager.StartNewFile(Path.Combine(modelRoot.ContextOutputDirectory, $"{modelRoot.EntityContainerName}{fileNameMarker}.cs"));
            WriteDbContext();

            // Context factory
            if (modelRoot.GenerateDbContextFactory)
            {
               ClearIndent();
               manager.StartNewFile(Path.Combine(modelRoot.ContextOutputDirectory, $"{modelRoot.EntityContainerName}Factory{fileNameMarker}.cs"));
               Output($"#nullable {(modelRoot.GenerateNullable ? "enable" : "disable")}");
               WriteDbContextFactory();
               Output($"#nullable {(modelRoot.GenerateNullable ? "enable" : "disable")}");
            }
         }

         /// <summary>
         /// Gets the list of additional using statements.
         /// </summary>
         protected override List<string> GetAdditionalUsingStatements()
         {
            List<string> result = new List<string>();
            List<string> attributeTypes = modelRoot.Classes.SelectMany(c => c.Attributes).Select(a => a.Type).Distinct().ToList();

            if (attributeTypes.Intersect(modelRoot.SpatialTypes).Any())
               result.Add("using NetTopologySuite.Geometries;");

            return result;
         }

         /// <summary>
         /// Gets the foreign keys of the association based on the provided list of foreign key columns
         /// </summary>
         /// <param name="association">The association to retrieve foreign keys from</param>
         /// <param name="foreignKeyColumns">The list of foreign key columns</param>
         /// <returns>An IEnumerable of the foreign keys</returns>
         protected virtual IEnumerable<string> GetForeignKeys(Association association, List<string> foreignKeyColumns)
         {
            // final collection of foreign key property names, real or shadow
            // shadow properties will be double quoted, real properties won't
            IEnumerable<string> result = new List<string>();

            // foreign key definitions always go in the table representing the Dependent end of the association
            // if there is no dependent end (i.e., many-to-many), there are no foreign keys
            ModelClass principal = association.Principal;
            ModelClass dependent = association.Dependent;

            if ((principal != null) && (dependent != null))
            {
               if (string.IsNullOrWhiteSpace(association.FKPropertyName))
                  result = principal.AllIdentityAttributes.Select(identity => $"\"{CreateShadowPropertyName(association, foreignKeyColumns, identity)}\"").ToList();
               else
               {
                  // defined properties
                  result = association.FKPropertyName.Split(',').Select(prop => "k." + prop.Trim()).ToList();
                  foreignKeyColumns.AddRange(result);
               }
            }

            return result;
         }

         /// <summary>
         /// Writes bidirectional dependent associations
         /// </summary>
         /// <param name="sourceInstance">The source instance</param>
         /// <param name="baseSegment">The base segment</param>
         /// <param name="visited">The visited associations list</param>
         /// <param name="depth">How deep we are in transient parentage</param>
         protected virtual void WriteBidirectionalDependentAssociations(ModelClass sourceInstance, string baseSegment, List<Association> visited, int depth = 0)
         {
            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (BidirectionalAssociation association in Association.GetLinksToTargets(sourceInstance)
                                                                        .OfType<BidirectionalAssociation>()
                                                                        .Where(x => x.Persistent && (!x.Target.Persistent || x.Target.IsDependentType)))
            {
               if (visited.Contains(association))
                  continue;

               visited.Add(association);

               List<string> segments = new List<string>();

               string separator = sourceInstance.ModelRoot.ShadowKeyNamePattern == ShadowKeyPattern.TableColumn
                                     ? string.Empty
                                     : "_";

               switch (association.TargetMultiplicity) // realized by property on source
               {
                  case Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroMany:
                     {
                        segments.Add(baseSegment);
                        segments.Add($"OwnsMany(p => p.{association.TargetPropertyName})");
                        segments.Add($"ToTable(\"{(string.IsNullOrEmpty(association.Target.TableName) ? association.Target.Name : association.Target.TableName)}\")");
                        Output(segments);

                        segments.Add(baseSegment);
                        segments.Add($"OwnsMany(p => p.{association.TargetPropertyName})");
                        segments.Add($"WithOwner(\"{association.SourcePropertyName}\")");
                        segments.Add($"HasForeignKey(\"{association.SourcePropertyName}{separator}Id\")");
                        Output(segments);

                        segments.Add(baseSegment);
                        segments.Add($"OwnsMany(p => p.{association.TargetPropertyName})");
                        segments.Add($"Property<{modelRoot.DefaultIdentityType}>(\"Id\")");

                        Output(segments);

                        segments.Add(baseSegment);
                        segments.Add($"OwnsMany(p => p.{association.TargetPropertyName})");
                        segments.Add("HasKey(\"Id\")");

                        Output(segments);

                        WriteBidirectionalDependentAssociations(association.Target, $"{baseSegment}.OwnsMany(p => p.{association.TargetPropertyName})", visited);

                        break;
                     }

                  case Sawczyn.EFDesigner.EFModel.Multiplicity.One:
                     {
                        segments.Add(baseSegment);
                        segments.Add($"OwnsOne(p => p.{association.TargetPropertyName})");
                        segments.Add($"WithOwner(p => p.{association.SourcePropertyName})");
                        Output(segments);

                        if (!string.IsNullOrEmpty(association.Target.TableName))
                        {
                           segments.Add(baseSegment);
                           segments.Add($"OwnsOne(p => p.{association.TargetPropertyName})");
                           segments.Add($"ToTable(\"{association.Target.TableName}\")");
                           Output(segments);
                        }

                        foreach (ModelAttribute modelAttribute in association.Target.AllAttributes)
                        {
                           segments.Add($"{baseSegment}.OwnsOne(p => p.{association.TargetPropertyName}).Property(p => p.{modelAttribute.Name})");

                           if ((modelAttribute.ColumnName != modelAttribute.Name) && !string.IsNullOrEmpty(modelAttribute.ColumnName))
                              segments.Add($"HasColumnName(\"{modelAttribute.ColumnName}\")");

                           if (modelAttribute.Required)
                              segments.Add("IsRequired()");

                           if (segments.Count > 1)
                              Output(segments);

                           segments.Clear();
                        }

                        WriteBidirectionalDependentAssociations(association.Target, $"{baseSegment}.OwnsOne(p => p.{association.TargetPropertyName})", visited);

                        break;
                     }

                  case Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroOne:
                     {
                        segments.Add(baseSegment);
                        segments.Add($"OwnsOne(p => p.{association.TargetPropertyName})");
                        segments.Add($"WithOwner(p => p.{association.SourcePropertyName})");
                        Output(segments);

                        if (!string.IsNullOrEmpty(association.Target.TableName))
                        {
                           segments.Add(baseSegment);
                           segments.Add($"OwnsOne(p => p.{association.TargetPropertyName})");
                           segments.Add($"ToTable(\"{association.Target.TableName}\")");
                           Output(segments);
                        }

                        foreach (ModelAttribute modelAttribute in association.Target.AllAttributes)
                        {
                           segments.Add($"{baseSegment}.OwnsOne(p => p.{association.TargetPropertyName}).Property(p => p.{modelAttribute.Name})");

                           if ((modelAttribute.ColumnName != modelAttribute.Name) && !string.IsNullOrEmpty(modelAttribute.ColumnName))
                              segments.Add($"HasColumnName(\"{modelAttribute.ColumnName}\")");

                           if (modelAttribute.Required)
                              segments.Add("IsRequired()");

                           if (segments.Count > 1)
                              Output(segments);

                           segments.Clear();
                        }

                        WriteBidirectionalDependentAssociations(association.Target, $"{baseSegment}.OwnsOne(p => p.{association.TargetPropertyName})", visited);

                        break;
                     }
               }
            }
         }

         /// <summary>
         /// Writes bidirectional non-dependent association elements for the given model class.
         /// </summary>
         /// <param name="modelClass">The model class for which to write the associations.</param>
         /// <param name="visited">A list of associations that have already been visited to avoid infinite recursion.</param>
         /// <param name="foreignKeyColumns">A list of foreign key columns.</param>
         protected virtual void WriteBidirectionalNonDependentAssociations(ModelClass modelClass, List<Association> visited, List<string> foreignKeyColumns)
         {
            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (BidirectionalAssociation association in Association.GetLinksToTargets(modelClass)
                                                                        .OfType<BidirectionalAssociation>()
                                                                        .Where(x => x.Persistent && !x.Target.IsDependentType))
            {
               if (visited.Contains(association))
                  continue;

               visited.Add(association);

               List<string> segments = new List<string>();
               bool required = false;

               segments.Add($"modelBuilder.Entity<{modelClass.FullName}>()");

               switch (association.TargetMultiplicity) // realized by property on source
               {
                  case Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroMany:
                     segments.Add($"HasMany<{association.Target.FullName}>(p => p.{association.TargetPropertyName})");

                     break;

                  case Sawczyn.EFDesigner.EFModel.Multiplicity.One:
                     segments.Add($"HasOne<{association.Target.FullName}>(p => p.{association.TargetPropertyName})");
                     required = modelClass == association.Principal;

                     break;

                  case Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroOne:
                     segments.Add($"HasOne<{association.Target.FullName}>(p => p.{association.TargetPropertyName})");

                     break;
               }

               switch (association.SourceMultiplicity) // realized by property on target, but no property on target
               {
                  case Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroMany:
                     segments.Add($"WithMany(p => p.{association.SourcePropertyName})");

                     if (association.TargetMultiplicity == Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroMany)
                     {
                        string tableMap = string.IsNullOrEmpty(association.JoinTableName)
                                             ? $"{association.Target.Name}_{association.SourcePropertyName}_x_{association.Source.Name}_{association.TargetPropertyName}"
                                             : association.JoinTableName;

                        segments.Add($"UsingEntity(x => x.ToTable(\"{tableMap}\"))");
                     }

                     break;

                  case Sawczyn.EFDesigner.EFModel.Multiplicity.One:
                     segments.Add($"WithOne(p => p.{association.SourcePropertyName})");
                     required = modelClass == association.Principal;

                     break;

                  case Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroOne:
                     segments.Add($"WithOne(p => p.{association.SourcePropertyName})");

                     break;
               }

               string foreignKeySegment = CreateForeignKeySegment(association, foreignKeyColumns);

               if (!string.IsNullOrEmpty(foreignKeySegment))
                  segments.Add(foreignKeySegment);

               WriteSourceDeleteBehavior(association, segments);

               if (required
                && ((association.SourceMultiplicity != Sawczyn.EFDesigner.EFModel.Multiplicity.One)
                 || (association.TargetMultiplicity != Sawczyn.EFDesigner.EFModel.Multiplicity.One)))
                  segments.Add("IsRequired()");

               Output(segments);
            }
         }

         /// <summary>
         /// Writes the DbContext constructors.
         /// </summary>
         protected void WriteContextConstructors()
         {
            if (!string.IsNullOrEmpty(modelRoot.ConnectionString) || !string.IsNullOrEmpty(modelRoot.ConnectionStringName))
            {
               string connectionString = string.IsNullOrEmpty(modelRoot.ConnectionString)
                                            ? $"Name={modelRoot.ConnectionStringName}"
                                            : modelRoot.ConnectionString;

               Output("/// <summary>");
               Output("/// Default connection string");
               Output("/// </summary>");
               Output($"public static string ConnectionString {{ get; set; }} = @\"{connectionString}\";");
               NL();
            }

            Output("/// <inheritdoc />");
            Output($"public {modelRoot.EntityContainerName}() : base()");
            Output("{");
            Output("}");
            NL();
            Output("/// <summary>");
            Output("///     <para>");
            Output("///         Initializes a new instance of the <see cref=\"T:Microsoft.EntityFrameworkCore.DbContext\" /> class using the specified options.");
            Output("///         The <see cref=\"M:Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)\" /> method will still be called to allow further");
            Output("///         configuration of the options.");
            Output("///     </para>");
            Output("/// </summary>");
            Output("/// <param name=\"options\">The options for this context.</param>");
            Output($"public {modelRoot.EntityContainerName}(DbContextOptions<{modelRoot.EntityContainerName}> options) : base(options)");
            Output("{");
            Output("}");
            NL();

            Output("partial void CustomInit(DbContextOptionsBuilder optionsBuilder);");
            NL();
         }

         /// <summary>
         /// Writes the DbContext file.
         /// </summary>
         protected virtual void WriteDbContext()
         {
            List<string> segments = new List<string>();
            List<ModelClass> classesWithTablesList = new List<ModelClass>();

            foreach (ModelClass mc in modelRoot.Classes)
            {
               if (mc.Persistent && !mc.IsQueryType && !mc.CustomAttributes.Contains("NotMapped") && mc.GenerateCode)
               {
                  switch (mc.InheritanceStrategy)
                  {
                     case CodeStrategy.TablePerType:
                        if (!mc.IsDependentType || !string.IsNullOrEmpty(mc.TableName) || !string.IsNullOrEmpty(mc.ViewName))
                        {
                           if (!mc.IsQueryType && !mc.IsDatabaseView)
                              classesWithTablesList.Add(mc);
                        }

                        break;

                     case CodeStrategy.TablePerConcreteType:
                        if (!mc.IsDependentType || !string.IsNullOrEmpty(mc.TableName) || !string.IsNullOrEmpty(mc.ViewName))
                        {
                           if (!mc.IsQueryType && !mc.IsDatabaseView && !mc.IsAbstract)
                              classesWithTablesList.Add(mc);
                        }

                        break;

                     case CodeStrategy.TablePerHierarchy:
                        if (!mc.IsDependentType || !string.IsNullOrEmpty(mc.TableName) && mc.Superclass == null)
                           classesWithTablesList.Add(mc);

                        break;
                  }
               }
            }

            ModelClass[] classesWithTables = classesWithTablesList.OrderBy(x => x.Name).ToArray();

            Output("using System;");
            Output("using System.Collections.Generic;");
            Output("using System.Linq;");
            Output("using System.ComponentModel.DataAnnotations.Schema;");
            Output("using Microsoft.EntityFrameworkCore;");
            NL();

            BeginNamespace(modelRoot.Namespace);

            WriteDbContextComments();

            string baseClass = string.IsNullOrWhiteSpace(modelRoot.BaseClass)
                                  ? "Microsoft.EntityFrameworkCore.DbContext"
                                  : modelRoot.BaseClass;

            Output($"{modelRoot.EntityContainerAccess.ToString().ToLower()} partial class {modelRoot.EntityContainerName} : {baseClass}");
            Output("{");

            if (classesWithTables.Any())
               WriteDbSets();

            WriteContextConstructors();

            if (!modelRoot.GenerateDbContextFactory)
               WriteOnConfiguring(segments);

            WriteOnModelCreate(segments, classesWithTables);

            Output("}");

            EndNamespace(modelRoot.Namespace);
         }

         /// <summary>
         /// Writes the DbContextFactory file.
         /// </summary>
         protected void WriteDbContextFactory()
         {
            Output("using System;");
            Output("using System.Collections.Generic;");
            Output("using System.Linq;");
            Output("using System.Text;");
            Output("using System.Threading.Tasks;");
            NL();

            Output("using Microsoft.EntityFrameworkCore;");
            Output("using Microsoft.EntityFrameworkCore.Design;");
            NL();

            BeginNamespace(modelRoot.Namespace);

            Output("/// <summary>");
            Output("/// A factory for creating derived DbContext instances. Implement this interface to enable design-time services for context ");
            Output("/// types that do not have a public default constructor. At design-time, derived DbContext instances can be created in order ");
            Output("/// to enable specific design-time experiences such as Migrations. Design-time services will automatically discover ");
            Output("/// implementations of this interface that are in the startup assembly or the same assembly as the derived context.");
            Output("/// </summary>");

            Output($"public partial class {modelRoot.EntityContainerName}DesignTimeFactory: IDesignTimeDbContextFactory<{modelRoot.EntityContainerName}>");
            Output("{");

            Output("/// <summary>");
            Output("/// Partial method to allow post-creation configuration of the DbContext after it's created");
            Output("/// but before it's returned.");
            Output("/// </summary>");
            Output($"partial void Initialize({modelRoot.EntityContainerName} dbContext);");
            NL();

            Output("/// <summary>Creates a new instance of a derived context.</summary>");
            Output("/// <param name=\"args\"> Arguments provided by the design-time service. </param>");
            Output($"/// <returns> An instance of <see cref=\"{modelRoot.Namespace}.{modelRoot.EntityContainerName}\" />.</returns>");
            Output($"public {modelRoot.EntityContainerName} CreateDbContext(string[] args)");
            Output("{");
            Output($"DbContextOptionsBuilder<{modelRoot.EntityContainerName}> optionsBuilder = new DbContextOptionsBuilder<{modelRoot.EntityContainerName}>();");
            NL();

            Output($"// Please provide the {modelRoot.EntityContainerName}.ConfigureOptions(optionsBuilder) in the partial class as");
            Output("//    public static void ConfigureOptions(DbContextOptionsBuilder optionsBuilder) {{ ... }}");
            Output("// If you have custom initialization for the context, you can then consolidate the code by defining the CustomInit partial as");
            Output("//    partial void CustomInit(DbContextOptionsBuilder optionsBuilder) => ConfigureOptions(optionsBuilder);");
            Output($"{modelRoot.EntityContainerName}.ConfigureOptions(optionsBuilder);");
            Output($"{modelRoot.EntityContainerName} result = new {modelRoot.EntityContainerName}(optionsBuilder.Options);");
            Output("Initialize(result);");
            NL();

            Output("return result;");
            Output("}");
            Output("}");
            NL();

            Output("/// <summary>");
            Output("///     Defines a factory for creating derived DbContext instances.");
            Output("/// </summary>");
            Output("/// <remarks>");
            Output("///     See <see href=\"https://aka.ms/efcore-docs-dbcontext-factory\">Using DbContextFactory</see> for more information.");
            Output("/// </remarks>");
            Output($"public partial class {modelRoot.EntityContainerName}Factory: IDbContextFactory<{modelRoot.EntityContainerName}>");
            Output("{");
            Output("/// <summary>");
            Output("/// Partial method to allow post-creation configuration of the DbContext after it's created");
            Output("/// but before it's returned.");
            Output("/// </summary>");
            Output($"partial void Initialize({modelRoot.EntityContainerName} dbContext);");
            NL();

            Output("/// <summary>");
            Output("///     <para>");
            Output("///         Creates a new <see cref=\"T:Microsoft.EntityFrameworkCore.DbContext\" /> instance.");
            Output("///     </para>");
            Output("///     <para>");
            Output("///         The caller is responsible for disposing the context; it will not be disposed by any dependency injection container.");
            Output("///     </para>");
            Output("/// </summary>");
            Output("/// <returns>A new context instance.</returns>");
            Output($"public {modelRoot.EntityContainerName} CreateDbContext()");
            Output("{");
            Output($"DbContextOptionsBuilder<{modelRoot.EntityContainerName}> optionsBuilder = new DbContextOptionsBuilder<{modelRoot.EntityContainerName}>();");
            NL();

            Output($"// Please provide the {modelRoot.EntityContainerName}.ConfigureOptions(optionsBuilder) in the partial class as");
            Output("//    public static void ConfigureOptions(DbContextOptionsBuilder optionsBuilder) {{ ... }}");
            Output("// If you have custom initialization for the context, you can then consolidate the code by defining the CustomInit partial as");
            Output("//    partial void CustomInit(DbContextOptionsBuilder optionsBuilder) => ConfigureOptions(optionsBuilder);");
            Output($"{modelRoot.EntityContainerName}.ConfigureOptions(optionsBuilder);");
            Output($"{modelRoot.EntityContainerName} result = new {modelRoot.EntityContainerName}(optionsBuilder.Options);");
            Output("Initialize(result);");
            NL();

            Output("return result;");
            Output("}");
            Output("}");

            EndNamespace(modelRoot.Namespace);
         }

         /// <summary>
         /// Writes the DbSet code for the DbContext.
         /// </summary>
         protected void WriteDbSets()
         {
            Output("#region DbSets");
            PluralizationService pluralizationService = ModelRoot.PluralizationService;

            foreach (ModelClass modelClass in modelRoot.Classes
                                                       .Where(x => !x.IsDependentType 
                                                                && x.Persistent 
                                                                && !x.IsDatabaseView
                                                                && !x.IsQueryType)
                                                       .OrderBy(x => x.Name))
            {
               string dbSetName = WriteDbSetHeader( modelClass );
               Output($"{modelRoot.DbSetAccess.ToString().ToLower()} virtual Microsoft.EntityFrameworkCore.DbSet<{modelClass.FullName}> {dbSetName} {{ get; set; }}");
            }

            foreach (ModelClass modelClass in modelRoot.Classes
                                                       .Where(x => x.IsQueryType || x.IsDatabaseView)
                                                       .OrderBy(x => x.Name))
            {
               string dbSetName = WriteDbSetHeader( modelClass );
               Output($"{modelRoot.DbSetAccess.ToString().ToLower()} virtual Microsoft.EntityFrameworkCore.DbSet<{modelClass.FullName}> {dbSetName} {{ get; set; }}");
            }

            NL();
            Output("#endregion DbSets");
            NL();

            string WriteDbSetHeader( ModelClass modelClass )
            {
               string dbSetName;

               if (!string.IsNullOrEmpty( modelClass.DbSetName ))
                  dbSetName = modelClass.DbSetName;
               else
               {
                  dbSetName = pluralizationService?.IsSingular( modelClass.Name ) == true
                                 ? pluralizationService.Pluralize( modelClass.Name )
                                 : modelClass.Name;
               }

               if (!string.IsNullOrEmpty( modelClass.Summary ))
               {
                  NL();
                  Output( "/// <summary>" );
                  WriteCommentBody( $"Repository for {modelClass.FullName} - {modelClass.Summary}" );
                  Output( "/// </summary>" );
               }

               return dbSetName;
            }
         }

         /// <summary>
         /// Writes the OnConfiguring method for the DbContext into the segments collection.
         /// </summary>
         /// <param name="segments">Container for output code</param>
         protected void WriteOnConfiguring(List<string> segments)
         {
            Output("/// <inheritdoc />");
            Output("protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)");
            Output("{");

            segments.Clear();

            if ((modelRoot.GetEntityFrameworkPackageVersionNum() >= 2.1) && modelRoot.LazyLoadingEnabled)
               segments.Add("UseLazyLoadingProxies()");

            if (segments.Any())
            {
               segments.Insert(0, "optionsBuilder");

               Output(segments);
               NL();
            }

            Output("CustomInit(optionsBuilder);");
            Output("}");
            NL();
         }

         /// <summary>
         /// Writes the necessary code for initializing the entity framework model on creation.
         /// </summary>
         /// <param name="segments">The list of code segments to append to.</param>
         /// <param name="classesWithTables">The array of classes with associated database tables.</param>
         protected virtual void WriteOnModelCreate(List<string> segments, ModelClass[] classesWithTables)
         {
            Output("partial void OnModelCreatingImpl(ModelBuilder modelBuilder);");
            Output("partial void OnModelCreatedImpl(ModelBuilder modelBuilder);");
            NL();

            Output("/// <summary>");
            Output("///     Override this method to further configure the model that was discovered by convention from the entity types");
            Output("///     exposed in <see cref=\"T:Microsoft.EntityFrameworkCore.DbSet`1\" /> properties on your derived context. The resulting model may be cached");
            Output("///     and re-used for subsequent instances of your derived context.");
            Output("/// </summary>");
            Output("/// <remarks>");
            Output("///     If a model is explicitly set on the options for this context (via <see cref=\"M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)\" />)");
            Output("///     then this method will not be run.");
            Output("/// </remarks>");
            Output("/// <param name=\"modelBuilder\">");
            Output("///     The builder being used to construct the model for this context. Databases (and other extensions) typically");
            Output("///     define extension methods on this object that allow you to configure aspects of the model that are specific");
            Output("///     to a given database.");
            Output("/// </param>");
            Output("protected override void OnModelCreating(ModelBuilder modelBuilder)");
            Output("{");
            Output("base.OnModelCreating(modelBuilder);");
            Output("OnModelCreatingImpl(modelBuilder);");
            NL();

            if (!string.IsNullOrEmpty(modelRoot.DatabaseSchema))
               Output($"modelBuilder.HasDefaultSchema(\"{modelRoot.DatabaseSchema}\");");

            List<Association> visited = new List<Association>();
            List<string> foreignKeyColumns = new List<string>();

            ConfigureModelClasses(segments, classesWithTables, foreignKeyColumns, visited);

            NL();

            Output("OnModelCreatedImpl(modelBuilder);");
            Output("}");
         }

         /// <summary>
         /// Writes the delete behavior for a bidirectional association's source role to the list of segments.
         /// </summary>
         /// <param name="association">The bidirectional association.</param>
         /// <param name="segments">The list of segments to add the delete behavior to.</param>
         protected virtual void WriteSourceDeleteBehavior(BidirectionalAssociation association, List<string> segments)
         {
            if (!association.Source.IsDependentType
             && !association.Target.IsDependentType
             && ((association.TargetRole == EndpointRole.Principal) || (association.SourceRole == EndpointRole.Principal)))
            {
               DeleteAction deleteAction = association.SourceRole == EndpointRole.Principal
                                              ? association.SourceDeleteAction
                                              : association.TargetDeleteAction;

               switch (deleteAction)
               {
                  case DeleteAction.None:
                     segments.Add("OnDelete(DeleteBehavior.Restrict)");

                     break;

                  case DeleteAction.Cascade:
                     segments.Add("OnDelete(DeleteBehavior.Cascade)");

                     break;
               }
            }
         }

         /// <summary>
         /// Writes the delete behavior for the target class of the given association into a list of segments.
         /// </summary>
         /// <param name="association">The Association object to write delete behavior for.</param>
         /// <param name="segments">The list of segments to contain the generated code.</param>
         protected virtual void WriteTargetDeleteBehavior(Association association, List<string> segments)
         {
            if (!association.Source.IsDependentType
             && !association.Target.IsDependentType
             && ((association.TargetRole == EndpointRole.Principal) || (association.SourceRole == EndpointRole.Principal)))
            {
               DeleteAction deleteAction = association.SourceRole == EndpointRole.Principal
                                              ? association.SourceDeleteAction
                                              : association.TargetDeleteAction;

               switch (deleteAction)
               {
                  case DeleteAction.None:
                     segments.Add("OnDelete(DeleteBehavior.Restrict)");

                     break;

                  case DeleteAction.Cascade:
                     segments.Add("OnDelete(DeleteBehavior.Cascade)");

                     break;
               }
            }
         }

         /// <summary>
         /// Writes the unidirectional dependent associations of a given ModelClass instance.
         /// </summary>
         /// <param name="sourceInstance">The ModelClass instance to search the unidirectional dependent associations.</param>
         /// <param name="baseSegment">The base segment to prepend the association's property segment.</param>
         /// <param name="visited">List of visited associations to avoid circular references.</param>
         /// <param name="depth">How deep we are in transient parentage</param>
         protected virtual void WriteUnidirectionalDependentAssociations(ModelClass sourceInstance, string baseSegment, List<Association> visited, int depth = 0)
         {
            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (UnidirectionalAssociation association in Association.GetLinksToTargets(sourceInstance)
                                                                         .OfType<UnidirectionalAssociation>()
                                                                         .Where(x => x.Persistent && (!x.Target.Persistent || x.Target.IsDependentType)))
            {
               if (visited.Contains(association))
                  continue;

               visited.Add(association);

               List<string> segments = new List<string>();

               string separator = sourceInstance.ModelRoot.ShadowKeyNamePattern == ShadowKeyPattern.TableColumn
                                     ? string.Empty
                                     : "_";

               switch (association.TargetMultiplicity) // realized by property on source
               {
                  case Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroMany:
                     {
                        segments.Add(baseSegment);
                        segments.Add($"OwnsMany(p => p.{association.TargetPropertyName})");
                        segments.Add($"WithOwner(\"{association.Source.Name}_{association.TargetPropertyName}\")");
                        segments.Add($"HasForeignKey(\"{association.Source.Name}_{association.TargetPropertyName}{separator}Id\")");
                        Output(segments);

                        segments.Add(baseSegment);
                        segments.Add($"OwnsMany(p => p.{association.TargetPropertyName})");
                        segments.Add($"Property<{modelRoot.DefaultIdentityType}>(\"Id\")");

                        Output(segments);

                        segments.Add(baseSegment);
                        segments.Add($"OwnsMany(p => p.{association.TargetPropertyName})");
                        segments.Add("HasKey(\"Id\")");

                        Output(segments);

                        WriteUnidirectionalDependentAssociations(association.Target, $"{baseSegment}.OwnsMany(p => p.{association.TargetPropertyName})", visited);

                        break;
                     }

                  case Sawczyn.EFDesigner.EFModel.Multiplicity.One:
                     {
                        foreach (ModelAttribute modelAttribute in association.Target.AllAttributes)
                        {
                           segments.Add($"{baseSegment}.OwnsOne(p => p.{association.TargetPropertyName}).Property(p => p.{modelAttribute.Name})");

                           if ((modelAttribute.ColumnName != modelAttribute.Name) && !string.IsNullOrEmpty(modelAttribute.ColumnName))
                              segments.Add($"HasColumnName(\"{modelAttribute.ColumnName}\")");

                           if (modelAttribute.Required)
                              segments.Add("IsRequired()");

                           Output(segments);
                        }

                        WriteUnidirectionalDependentAssociations(association.Target, $"{baseSegment}.OwnsOne(p => p.{association.TargetPropertyName})", visited);

                        break;
                     }

                  case Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroOne:
                     {
                        foreach (ModelAttribute modelAttribute in association.Target.AllAttributes)
                        {
                           segments.Add($"{baseSegment}.OwnsOne(p => p.{association.TargetPropertyName}).Property(p => p.{modelAttribute.Name})");

                           if ((modelAttribute.ColumnName != modelAttribute.Name) && !string.IsNullOrEmpty(modelAttribute.ColumnName))
                              segments.Add($"HasColumnName(\"{modelAttribute.ColumnName}\")");

                           if (modelAttribute.Required)
                              segments.Add("IsRequired()");

                           Output(segments);
                        }

                        WriteUnidirectionalDependentAssociations(association.Target, $"{baseSegment}.OwnsOne(p => p.{association.TargetPropertyName})", visited);

                        break;
                     }
               }
            }
         }

         /// <summary>
         /// Writes unidirectional non-dependent associations
         /// </summary>
         /// <param name="modelClass">The model class</param>
         /// <param name="visited">The list of already-visited association</param>
         /// <param name="foreignKeyColumns">The list of foreign key columns</param>
         protected virtual void WriteUnidirectionalNonDependentAssociations(ModelClass modelClass, List<Association> visited, List<string> foreignKeyColumns)
         {
            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (UnidirectionalAssociation association in Association.GetLinksToTargets(modelClass)
                                                                         .OfType<UnidirectionalAssociation>()
                                                                         .Where(x => x.Persistent && !x.Target.IsDependentType))
            {
               if (visited.Contains(association))
                  continue;

               visited.Add(association);

               List<string> segments = new List<string>();
               bool required = false;

               segments.Add($"modelBuilder.Entity<{modelClass.FullName}>()");

               switch (association.TargetMultiplicity) // realized by property on source
               {
                  case Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroMany:
                     segments.Add($"HasMany<{association.Target.FullName}>(p => p.{association.TargetPropertyName})");

                     break;

                  case Sawczyn.EFDesigner.EFModel.Multiplicity.One:
                     segments.Add($"HasOne<{association.Target.FullName}>(p => p.{association.TargetPropertyName})");
                     required = modelClass == association.Principal;

                     break;

                  case Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroOne:
                     segments.Add($"HasOne<{association.Target.FullName}>(p => p.{association.TargetPropertyName})");

                     break;
               }

               switch (association.SourceMultiplicity) // realized by property on target, but no property on target
               {
                  case Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroMany:
                     segments.Add("WithMany()");

                     if (association.TargetMultiplicity == Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroMany)
                     {
                        string tableMap = string.IsNullOrEmpty(association.JoinTableName)
                                             ? $"{association.Target.Name}_x_{association.Source.Name}_{association.TargetPropertyName}"
                                             : association.JoinTableName;

                        segments.Add($"UsingEntity(x => x.ToTable(\"{tableMap}\"))");
                     }

                     break;

                  case Sawczyn.EFDesigner.EFModel.Multiplicity.One:
                     segments.Add("WithOne()");
                     required = modelClass == association.Principal;

                     break;

                  case Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroOne:
                     segments.Add("WithOne()");

                     break;
               }

               string foreignKeySegment = CreateForeignKeySegment(association, foreignKeyColumns);

               if (!string.IsNullOrEmpty(foreignKeySegment))
                  segments.Add(foreignKeySegment);
               else if (association.Is(Sawczyn.EFDesigner.EFModel.Multiplicity.One, Sawczyn.EFDesigner.EFModel.Multiplicity.One)
                     || association.Is(Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroOne, Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroOne))
                  segments.Add($"HasForeignKey<{association.Dependent.FullName}>()");

               WriteTargetDeleteBehavior(association, segments);

               if (required
                && ((association.SourceMultiplicity != Sawczyn.EFDesigner.EFModel.Multiplicity.One)
                 || (association.TargetMultiplicity != Sawczyn.EFDesigner.EFModel.Multiplicity.One)))
                  segments.Add("IsRequired()");

               Output(segments);
            }
         }
      }

      #endregion Template
   }
}