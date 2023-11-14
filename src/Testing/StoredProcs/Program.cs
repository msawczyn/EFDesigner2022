using Microsoft.EntityFrameworkCore;

namespace StoredProcs
{
   internal static class Program
   {
      private static void Main()
      {
         ProcContextFactory factory = new ProcContextFactory();
         ProcContext dbContext = factory.CreateDbContext();

         dbContext.Database.EnsureDeleted();
         dbContext.Database.Migrate();

         Grade[] grades = new[] { "A", "B", "C", "D", "F" }.Select(g => new Grade { Name = g }).ToArray();
         dbContext.Grades.AddRange(grades);

         dbContext.Students.Add(new Student { FirstName = "John", LastName = "Doe", Grade = grades[0] });
         dbContext.Students.Add(new Student { FirstName = "Jane", LastName = "Doe", Grade = grades[1] });
         dbContext.Students.Add(new Student { FirstName = "Jack", LastName = "Doe", Grade = grades[2] });

         dbContext.SaveChanges();

         dbContext.Students.FromSql($"GetStarStudents").ToList().ForEach(s => System.Console.WriteLine($"{s.FirstName} {s.LastName}"));
      }
   }
}