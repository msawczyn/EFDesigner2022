using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transient_Owned.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransientDetails_Masters_MasterId",
                schema: "dbo",
                table: "TransientDetails");

            migrationBuilder.AddColumn<string>(
                name: "StringArray",
                schema: "dbo",
                table: "Masters",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TransientDetails_Masters_MasterId",
                schema: "dbo",
                table: "TransientDetails",
                column: "MasterId",
                principalSchema: "dbo",
                principalTable: "Masters",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransientDetails_Masters_MasterId",
                schema: "dbo",
                table: "TransientDetails");

            migrationBuilder.DropColumn(
                name: "StringArray",
                schema: "dbo",
                table: "Masters");

            migrationBuilder.AddForeignKey(
                name: "FK_TransientDetails_Masters_MasterId",
                schema: "dbo",
                table: "TransientDetails",
                column: "MasterId",
                principalSchema: "dbo",
                principalTable: "Masters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
