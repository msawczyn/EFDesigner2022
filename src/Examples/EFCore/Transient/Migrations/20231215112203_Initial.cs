using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transient_Owned.Migrations
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
                name: "PersistentDetailSequence",
                schema: "dbo");

            migrationBuilder.CreateTable(
                name: "Masters",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StringCollection = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Foo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransientDetailAsJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Masters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersistentDetails",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[PersistentDetailSequence]"),
                    Bar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MasterPersistentDetailsId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersistentDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersistentDetails_Masters_MasterPersistentDetailsId",
                        column: x => x.MasterPersistentDetailsId,
                        principalSchema: "dbo",
                        principalTable: "Masters",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TransientDetails",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Baz = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "Empty"),
                    MasterId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransientDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransientDetails_Masters_MasterId",
                        column: x => x.MasterId,
                        principalSchema: "dbo",
                        principalTable: "Masters",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersistentDetails_MasterPersistentDetailsId",
                schema: "dbo",
                table: "PersistentDetails",
                column: "MasterPersistentDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_TransientDetails_MasterId",
                schema: "dbo",
                table: "TransientDetails",
                column: "MasterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersistentDetails",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TransientDetails",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Masters",
                schema: "dbo");

            migrationBuilder.DropSequence(
                name: "PersistentDetailSequence",
                schema: "dbo");
        }
    }
}
