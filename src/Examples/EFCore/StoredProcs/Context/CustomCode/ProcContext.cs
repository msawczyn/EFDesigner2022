using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace StoredProcs
{
   public partial class ProcContext
   {
      public static void ConfigureOptions(DbContextOptionsBuilder optionsBuilder)
      {
         optionsBuilder.LogTo(x => Trace.WriteLine(x));
         optionsBuilder.UseSqlServer(ConnectionString);
      }
   }
}

