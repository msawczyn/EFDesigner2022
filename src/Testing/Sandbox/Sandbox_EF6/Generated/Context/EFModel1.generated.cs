//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
//
//     Produced by Entity Framework Visual Editor v4.2.2.0
//     Source:                    https://github.com/msawczyn/EFDesigner
//     Visual Studio Marketplace: https://marketplace.visualstudio.com/items?itemName=michaelsawczyn.EFDesigner
//     Documentation:             https://msawczyn.github.io/EFDesigner/
//     License (MIT):             https://github.com/msawczyn/EFDesigner/blob/master/LICENSE
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;

namespace Sandbox_EF6
{
   /// <inheritdoc/>
   public partial class EFModel1 : DbContext
   {
      #region DbSets
      public virtual System.Data.Entity.DbSet<global::Sandbox_EF6.Entity1> Entity1 { get; set; }
      public virtual System.Data.Entity.DbSet<global::Sandbox_EF6.Entity2> Entity2 { get; set; }
      #endregion DbSets

      #region Constructors

      partial void CustomInit();

      #warning Default constructor not generated for EFModel1 since no default connection string was specified in the model

      /// <inheritdoc />
      public EFModel1(string connectionString) : base(connectionString)
      {
         Configuration.LazyLoadingEnabled = true;
         Configuration.ProxyCreationEnabled = true;
         System.Data.Entity.Database.SetInitializer<EFModel1>(new EFModel1DatabaseInitializer());
         CustomInit();
      }

      /// <inheritdoc />
      public EFModel1(string connectionString, System.Data.Entity.Infrastructure.DbCompiledModel model) : base(connectionString, model)
      {
         Configuration.LazyLoadingEnabled = true;
         Configuration.ProxyCreationEnabled = true;
         System.Data.Entity.Database.SetInitializer<EFModel1>(new EFModel1DatabaseInitializer());
         CustomInit();
      }

      /// <inheritdoc />
      public EFModel1(System.Data.Common.DbConnection existingConnection, bool contextOwnsConnection) : base(existingConnection, contextOwnsConnection)
      {
         Configuration.LazyLoadingEnabled = true;
         Configuration.ProxyCreationEnabled = true;
         System.Data.Entity.Database.SetInitializer<EFModel1>(new EFModel1DatabaseInitializer());
         CustomInit();
      }

      /// <inheritdoc />
      public EFModel1(System.Data.Common.DbConnection existingConnection, System.Data.Entity.Infrastructure.DbCompiledModel model, bool contextOwnsConnection) : base(existingConnection, model, contextOwnsConnection)
      {
         Configuration.LazyLoadingEnabled = true;
         Configuration.ProxyCreationEnabled = true;
         System.Data.Entity.Database.SetInitializer<EFModel1>(new EFModel1DatabaseInitializer());
         CustomInit();
      }

      /// <inheritdoc />
      public EFModel1(System.Data.Entity.Infrastructure.DbCompiledModel model) : base(model)
      {
         Configuration.LazyLoadingEnabled = true;
         Configuration.ProxyCreationEnabled = true;
         System.Data.Entity.Database.SetInitializer<EFModel1>(new EFModel1DatabaseInitializer());
         CustomInit();
      }

      /// <inheritdoc />
      public EFModel1(System.Data.Entity.Core.Objects.ObjectContext objectContext, bool dbContextOwnsObjectContext) : base(objectContext, dbContextOwnsObjectContext)
      {
         Configuration.LazyLoadingEnabled = true;
         Configuration.ProxyCreationEnabled = true;
         System.Data.Entity.Database.SetInitializer<EFModel1>(new EFModel1DatabaseInitializer());
         CustomInit();
      }

      #endregion Constructors

      partial void OnModelCreatingImpl(System.Data.Entity.DbModelBuilder modelBuilder);
      partial void OnModelCreatedImpl(System.Data.Entity.DbModelBuilder modelBuilder);

      /// <inheritdoc />
      protected override void OnModelCreating(System.Data.Entity.DbModelBuilder modelBuilder)
      {
         base.OnModelCreating(modelBuilder);
         OnModelCreatingImpl(modelBuilder);

         modelBuilder.HasDefaultSchema("dbo");

         modelBuilder.Entity<global::Sandbox_EF6.Entity1>()
                     .ToTable("Entity1")
                     .HasKey(t => t.Id);
         modelBuilder.Entity<global::Sandbox_EF6.Entity1>()
                     .Property(t => t.Id)
                     .IsRequired()
                     .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

         modelBuilder.Entity<global::Sandbox_EF6.Entity2>()
                     .ToTable("Entity2")
                     .HasKey(t => t.Id);
         modelBuilder.Entity<global::Sandbox_EF6.Entity2>()
                     .Property(t => t.Id)
                     .IsRequired()
                     .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
         modelBuilder.Entity<global::Sandbox_EF6.Entity2>()
                     .HasRequired(x => x.Entity1)
                     .WithMany(x => x.Entity2)
                     .Map(x => x.MapKey("Entity1Id"));

         OnModelCreatedImpl(modelBuilder);
      }
   }
}
