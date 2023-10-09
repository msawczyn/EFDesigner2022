using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sandbox_EFCore_Test.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Entity1",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entity1", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Entity2",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Discriminator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Property1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entity2", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Entity3",
                schema: "dbo",
                columns: table => new
                {
                    Entity1Id = table.Column<long>(type: "bigint", nullable: false),
                    Entity2Id = table.Column<long>(type: "bigint", nullable: false),
                    Id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entity3", x => new { x.Entity1Id, x.Entity2Id });
                    table.ForeignKey(
                        name: "FK_Entity3_Entity1_Entity1Id",
                        column: x => x.Entity1Id,
                        principalSchema: "dbo",
                        principalTable: "Entity1",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Entity3_Entity2_Entity2Id",
                        column: x => x.Entity2Id,
                        principalSchema: "dbo",
                        principalTable: "Entity2",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entity3_Entity2Id",
                schema: "dbo",
                table: "Entity3",
                column: "Entity2Id");

            migrationBuilder.CreateIndex(
                name: "IX_Entity3_Id",
                schema: "dbo",
                table: "Entity3",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Entity3",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Entity1",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Entity2",
                schema: "dbo");
        }
    }
}
