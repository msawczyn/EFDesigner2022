using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoredProcs.Migrations
{
   /// <inheritdoc />
   public partial class AddStoredProcedure : Migration
   {
      private string Create = @"
CREATE PROCEDURE FirstGraders 
	@SchoolId int
AS
BEGIN
	SELECT Students.Id, Students.GPA, Students.FirstName, Students.LastName, Students.GradeId
   FROM Students INNER JOIN
        Grade ON Students.GradeId = Grade.Id INNER JOIN
        Schools ON Grade.SchoolId = Schools.Id
   WHERE (Students.GradeId = 1) AND (Grade.Name = N'First') AND (Schools.Id = @SchoolId)
END
";
      private string Drop = "DROP PROCEDURE FirstGraders";

      /// <inheritdoc />
      protected override void Up(MigrationBuilder migrationBuilder)
      {
         migrationBuilder.Sql(Create);
      }

      /// <inheritdoc />
      protected override void Down(MigrationBuilder migrationBuilder)
      {
         migrationBuilder.Sql(Drop);
      }
   }
}
