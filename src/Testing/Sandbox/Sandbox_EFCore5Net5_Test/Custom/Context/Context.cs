using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Sandbox_EFCore_Test
{
   /// <inheritdoc/>
   partial class EFModel1
   {
      public EFModel1() : base(Options)
      {
         //System.Diagnostics.Debugger.Launch();
      }

      public static void ConfigureOptions(DbContextOptionsBuilder optionsBuilder)
      {
         optionsBuilder.UseSqlServer(ConnectionString);
      }

      partial void CustomInit(DbContextOptionsBuilder optionsBuilder) => ConfigureOptions(optionsBuilder);

      public static DbContextOptions Options
      {
         get
         {
            DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(ConnectionString);
            return optionsBuilder.Options;
         }
      }
   }
}
