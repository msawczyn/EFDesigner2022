//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
//
//     Produced by Entity Framework Visual Editor v4.2.5.1
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
using Microsoft.EntityFrameworkCore;

namespace Testing
{
   /// <inheritdoc/>
   public partial class EFModel1 : DbContext
   {
      #region DbSets
      public virtual Microsoft.EntityFrameworkCore.DbSet<global::Testing.Entity1> Entity1 { get; set; }
      public virtual Microsoft.EntityFrameworkCore.DbSet<global::Testing.Entity2> Entity2 { get; set; }

      #endregion DbSets

      /// <summary>
      /// Default connection string
      /// </summary>
      public static string ConnectionString { get; set; } = @"Data Source=.;Initial Catalog=Sandbox;Integrated Security=True;Persist Security Info=True;MultipleActiveResultSets=True;TrustServerCertificate=True";

      /// <summary>
      ///     <para>
      ///         Initializes a new instance of the <see cref="T:Microsoft.EntityFrameworkCore.DbContext" /> class using the specified options.
      ///         The <see cref="M:Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)" /> method will still be called to allow further
      ///         configuration of the options.
      ///     </para>
      /// </summary>
      /// <param name="options">The options for this context.</param>
      public EFModel1(DbContextOptions<EFModel1> options) : base(options)
      {
      }

      partial void CustomInit(DbContextOptionsBuilder optionsBuilder);

      partial void OnModelCreatingImpl(ModelBuilder modelBuilder);
      partial void OnModelCreatedImpl(ModelBuilder modelBuilder);

      /// <summary>
      ///     Override this method to further configure the model that was discovered by convention from the entity types
      ///     exposed in <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> properties on your derived context. The resulting model may be cached
      ///     and re-used for subsequent instances of your derived context.
      /// </summary>
      /// <remarks>
      ///     If a model is explicitly set on the options for this context (via <see cref="M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)" />)
      ///     then this method will not be run.
      /// </remarks>
      /// <param name="modelBuilder">
      ///     The builder being used to construct the model for this context. Databases (and other extensions) typically
      ///     define extension methods on this object that allow you to configure aspects of the model that are specific
      ///     to a given database.
      /// </param>
      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
         base.OnModelCreating(modelBuilder);
         OnModelCreatingImpl(modelBuilder);

         modelBuilder.HasDefaultSchema("dbo");

         modelBuilder.Entity<global::Testing.Entity1>()
                     .ToTable("Entity1")
                     .HasKey(t => new { t.Id, t.Property1 });
         modelBuilder.Entity<global::Testing.Entity1>()
                     .Property(t => t.Id)
                     .ValueGeneratedOnAdd()
                     .IsRequired();
         modelBuilder.Entity<global::Testing.Entity1>()
                     .Property(t => t.Property1)
                     .ValueGeneratedOnAdd()
                     .IsRequired();
         modelBuilder.Entity<global::Testing.Entity1>()
                     .HasMany<global::Testing.Entity2>(p => p.Entity2)
                     .WithMany(p => p.Entity1)
                     .UsingEntity<Dictionary<string, object>>(right => right.HasOne<global::Testing.Entity2>().WithMany().HasForeignKey("Entity2_Id").OnDelete(DeleteBehavior.Cascade),left => left.HasOne<global::Testing.Entity1>().WithMany().HasForeignKey("Entity1_Id", "Entity1_Property1").OnDelete(DeleteBehavior.Cascade),join => join.ToTable("Entity2_x_Entity1"));

         modelBuilder.Entity<global::Testing.Entity2>()
                     .ToTable("Entity2")
                     .HasKey(t => t.Id);
         modelBuilder.Entity<global::Testing.Entity2>()
                     .Property(t => t.Id)
                     .ValueGeneratedOnAdd()
                     .IsRequired();

         OnModelCreatedImpl(modelBuilder);
      }
   }
}
