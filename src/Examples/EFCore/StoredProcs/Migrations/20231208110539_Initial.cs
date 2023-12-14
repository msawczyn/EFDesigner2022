using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoredProcs.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateSequence(
                name: "AdultSequence",
                schema: "dbo");

            migrationBuilder.CreateSequence(
                name: "GradeSequence",
                schema: "dbo");

            migrationBuilder.CreateSequence(
                name: "SchoolSequence",
                schema: "dbo");

            migrationBuilder.CreateSequence(
                name: "StudentSequence",
                schema: "dbo");

            migrationBuilder.CreateTable(
                name: "Adult",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[AdultSequence]"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adult", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Schools",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[SchoolSequence]"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schools", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Grade",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[GradeSequence]"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SchoolId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grade", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Grade_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "dbo",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[StudentSequence]"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GradeId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_Grade_GradeId",
                        column: x => x.GradeId,
                        principalSchema: "dbo",
                        principalTable: "Grade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Adult_x_Student",
                schema: "dbo",
                columns: table => new
                {
                    Adult_Id = table.Column<long>(type: "bigint", nullable: false),
                    Student_Id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adult_x_Student", x => new { x.Adult_Id, x.Student_Id });
                    table.ForeignKey(
                        name: "FK_Adult_x_Student_Adult_Adult_Id",
                        column: x => x.Adult_Id,
                        principalSchema: "dbo",
                        principalTable: "Adult",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Adult_x_Student_Students_Student_Id",
                        column: x => x.Student_Id,
                        principalSchema: "dbo",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Adult_FirstName",
                schema: "dbo",
                table: "Adult",
                column: "FirstName");

            migrationBuilder.CreateIndex(
                name: "IX_Adult_LastName",
                schema: "dbo",
                table: "Adult",
                column: "LastName");

            migrationBuilder.CreateIndex(
                name: "IX_Adult_x_Student_Student_Id",
                schema: "dbo",
                table: "Adult_x_Student",
                column: "Student_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Grade_Name",
                schema: "dbo",
                table: "Grade",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Grade_SchoolId",
                schema: "dbo",
                table: "Grade",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_Name",
                schema: "dbo",
                table: "Schools",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_FirstName",
                schema: "dbo",
                table: "Students",
                column: "FirstName");

            migrationBuilder.CreateIndex(
                name: "IX_Students_GradeId",
                schema: "dbo",
                table: "Students",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_LastName",
                schema: "dbo",
                table: "Students",
                column: "LastName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Adult_x_Student",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Adult",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Students",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Grade",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Schools",
                schema: "dbo");

            migrationBuilder.DropSequence(
                name: "AdultSequence",
                schema: "dbo");

            migrationBuilder.DropSequence(
                name: "GradeSequence",
                schema: "dbo");

            migrationBuilder.DropSequence(
                name: "SchoolSequence",
                schema: "dbo");

            migrationBuilder.DropSequence(
                name: "StudentSequence",
                schema: "dbo");
        }
    }
}
