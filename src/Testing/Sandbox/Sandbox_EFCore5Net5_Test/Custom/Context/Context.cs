using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Credit.API.Domain_RE.Models_RE
{
   /// <inheritdoc/>
   partial class CreditContext
   {
      public CreditContext() : base(Options)
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
