using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Testing.Migrations
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
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Property1 = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entity1", x => new { x.Id, x.Property1 });
                });

            migrationBuilder.CreateTable(
                name: "Entity2",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entity2", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Entity2_x_Entity1",
                schema: "dbo",
                columns: table => new
                {
                    Entity2_Id = table.Column<long>(type: "bigint", nullable: false),
                    Entity1_Id = table.Column<long>(type: "bigint", nullable: false),
                    Entity1_Property1 = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entity2_x_Entity1", x => new { x.Entity2_Id, x.Entity1_Id, x.Entity1_Property1 });
                    table.ForeignKey(
                        name: "FK_Entity2_x_Entity1_Entity1_Entity1_Id_Entity1_Property1",
                        columns: x => new { x.Entity1_Id, x.Entity1_Property1 },
                        principalSchema: "dbo",
                        principalTable: "Entity1",
                        principalColumns: new[] { "Id", "Property1" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Entity2_x_Entity1_Entity2_Entity2_Id",
                        column: x => x.Entity2_Id,
                        principalSchema: "dbo",
                        principalTable: "Entity2",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entity2_x_Entity1_Entity1_Id_Entity1_Property1",
                schema: "dbo",
                table: "Entity2_x_Entity1",
                columns: new[] { "Entity1_Id", "Entity1_Property1" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Entity2_x_Entity1",
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
