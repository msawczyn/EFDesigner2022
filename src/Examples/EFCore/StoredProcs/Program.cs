using Microsoft.EntityFrameworkCore;

namespace StoredProcs
{
   internal static class Program
   {
      private static void Main()
      {
         using (ProcContext dbContext = new ProcContextFactory().CreateDbContext())
         {
            SetupDatabase(dbContext);
         }

         using (ProcContext dbContext = new ProcContextFactory().CreateDbContext())
         {
            School school = dbContext.Schools.Find(1);
            school.FirstGraders.ToList().ForEach(s => Console.WriteLine(s.FirstName));
         }
      }

      private static void SetupDatabase(ProcContext dbContext)
      {
         dbContext.Database.EnsureDeleted();
         dbContext.Database.Migrate();

         School school = new School("Doe Academy");
         dbContext.Schools.Add(school);

         Grade[] grades = new[] { "First", "Second", "Third" }.Select(g => new Grade(g, school)).ToArray();
         dbContext.Grades.AddRange(grades);

         List<Student> students = new List<Student>
         {
            new Student("John", "Doe", grades[0]),
            new Student("Jimmy", "Doe", grades[0]),
            new Student("Jane", "Doe", grades[1]),
            new Student("Jack", "Doe", grades[2])
         };

         dbContext.Students.AddRange(students);

         Adult parent = new Adult("William", "Doe");
         dbContext.Adults.Add(parent);

         students.ForEach(s => parent.Children.Add(s));

         dbContext.SaveChanges();

      }
   }
}