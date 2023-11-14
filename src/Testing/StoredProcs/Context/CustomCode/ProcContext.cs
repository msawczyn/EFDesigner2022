using Microsoft.EntityFrameworkCore;

namespace StoredProcs
{
   public partial class ProcContext
   {
      public static void ConfigureOptions(DbContextOptionsBuilder optionsBuilder)
      {
         optionsBuilder.UseSqlServer(ConnectionString);
      }

      partial void OnModelCreatedImpl(ModelBuilder modelBuilder)
      {
         modelBuilder.Entity<global::StoredProcs.Student>()
                     .Property(t => t.LastName)
                     .HasMaxLength(50)
                     .IsRequired();

      }
   }
}