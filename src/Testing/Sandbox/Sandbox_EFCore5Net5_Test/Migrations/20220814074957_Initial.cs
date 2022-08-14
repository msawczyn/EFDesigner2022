using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sandbox_EFCore5Net5_Test.Migrations
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
                name: "AnEntities",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Property1 = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Entity4",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entity4", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Entity5",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entity5", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestDatas",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestString = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestDatas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Entity3",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TheFK = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entity3", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entity3_Entity4_TheFK",
                        column: x => x.TheFK,
                        principalSchema: "dbo",
                        principalTable: "Entity4",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Entity6",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TheFK = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entity6", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entity6_Entity5_TheFK",
                        column: x => x.TheFK,
                        principalSchema: "dbo",
                        principalTable: "Entity5",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Entity2",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FKto1 = table.Column<long>(type: "bigint", nullable: false),
                    FKto6 = table.Column<long>(type: "bigint", nullable: false),
                    Property1 = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entity2", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entity2_Entity1_FKto1",
                        column: x => x.FKto1,
                        principalSchema: "dbo",
                        principalTable: "Entity1",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Entity2_Entity6_FKto6",
                        column: x => x.FKto6,
                        principalSchema: "dbo",
                        principalTable: "Entity6",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entity2_FKto1",
                schema: "dbo",
                table: "Entity2",
                column: "FKto1");

            migrationBuilder.CreateIndex(
                name: "IX_Entity2_FKto6",
                schema: "dbo",
                table: "Entity2",
                column: "FKto6");

            migrationBuilder.CreateIndex(
                name: "IX_Entity3_TheFK",
                schema: "dbo",
                table: "Entity3",
                column: "TheFK");

            migrationBuilder.CreateIndex(
                name: "IX_Entity6_TheFK",
                schema: "dbo",
                table: "Entity6",
                column: "TheFK");

            migrationBuilder.CreateIndex(
                name: "IX_TestDatas_TestString",
                schema: "dbo",
                table: "TestDatas",
                column: "TestString",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnEntities",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Entity2",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Entity3",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TestDatas",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Entity6",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Entity4",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Entity5",
                schema: "dbo");
        }
    }
}
