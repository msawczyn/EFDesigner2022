//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
//
//     Produced by Entity Framework Visual Editor v4.2.6.1
//     Source:                    https://github.com/msawczyn/EFDesigner
//     Visual Studio Marketplace: https://marketplace.visualstudio.com/items?itemName=michaelsawczyn.EFDesigner
//     Documentation:             https://msawczyn.github.io/EFDesigner/
//     License (MIT):             https://github.com/msawczyn/EFDesigner/blob/master/LICENSE
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StoredProcs
{
   /// <summary>
   /// A factory for creating derived DbContext instances. Implement this interface to enable design-time services for context 
   /// types that do not have a public default constructor. At design-time, derived DbContext instances can be created in order 
   /// to enable specific design-time experiences such as Migrations. Design-time services will automatically discover 
   /// implementations of this interface that are in the startup assembly or the same assembly as the derived context.
   /// </summary>
   public partial class ProcContextDesignTimeFactory: IDesignTimeDbContextFactory<ProcContext>
   {
      /// <summary>
      /// Partial method to allow post-creation configuration of the DbContext after it's created
      /// but before it's returned.
      /// </summary>
      partial void Initialize(ProcContext dbContext);

      /// <summary>Creates a new instance of a derived context.</summary>
      /// <param name="args"> Arguments provided by the design-time service. </param>
      /// <returns> An instance of <see cref="StoredProcs.ProcContext" />.</returns>
      public ProcContext CreateDbContext(string[] args)
      {
         DbContextOptionsBuilder<ProcContext> optionsBuilder = new DbContextOptionsBuilder<ProcContext>();

         // Please provide the ProcContext.ConfigureOptions(optionsBuilder) in the partial class as
         //    public static void ConfigureOptions(DbContextOptionsBuilder optionsBuilder) {{ ... }}
         // If you have custom initialization for the context, you can then consolidate the code by defining the CustomInit partial as
         //    partial void CustomInit(DbContextOptionsBuilder optionsBuilder) => ConfigureOptions(optionsBuilder);
         ProcContext.ConfigureOptions(optionsBuilder);
         ProcContext result = new ProcContext(optionsBuilder.Options);
         Initialize(result);

         return result;
      }
   }

   /// <summary>
   ///     Defines a factory for creating derived DbContext instances.
   /// </summary>
   /// <remarks>
   ///     See <see href="https://aka.ms/efcore-docs-dbcontext-factory">Using DbContextFactory</see> for more information.
   /// </remarks>
   public partial class ProcContextFactory: IDbContextFactory<ProcContext>
   {
      /// <summary>
      /// Partial method to allow post-creation configuration of the DbContext after it's created
      /// but before it's returned.
      /// </summary>
      partial void Initialize(ProcContext dbContext);

      /// <summary>
      ///     <para>
      ///         Creates a new <see cref="T:Microsoft.EntityFrameworkCore.DbContext" /> instance.
      ///     </para>
      ///     <para>
      ///         The caller is responsible for disposing the context; it will not be disposed by any dependency injection container.
      ///     </para>
      /// </summary>
      /// <returns>A new context instance.</returns>
      public ProcContext CreateDbContext()
      {
         DbContextOptionsBuilder<ProcContext> optionsBuilder = new DbContextOptionsBuilder<ProcContext>();

         // Please provide the ProcContext.ConfigureOptions(optionsBuilder) in the partial class as
         //    public static void ConfigureOptions(DbContextOptionsBuilder optionsBuilder) {{ ... }}
         // If you have custom initialization for the context, you can then consolidate the code by defining the CustomInit partial as
         //    partial void CustomInit(DbContextOptionsBuilder optionsBuilder) => ConfigureOptions(optionsBuilder);
         ProcContext.ConfigureOptions(optionsBuilder);
         ProcContext result = new ProcContext(optionsBuilder.Options);
         Initialize(result);

         return result;
      }
   }
}
