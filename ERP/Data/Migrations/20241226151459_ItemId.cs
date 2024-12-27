using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class ItemId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                table: "ProjectFile",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Project",
                type: "bit",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectFile_ItemId",
                table: "ProjectFile",
                column: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectFile_Item_ItemId",
                table: "ProjectFile",
                column: "ItemId",
                principalTable: "Item",
                principalColumn: "ItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectFile_Item_ItemId",
                table: "ProjectFile");

            migrationBuilder.DropIndex(
                name: "IX_ProjectFile_ItemId",
                table: "ProjectFile");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "ProjectFile");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Project");
        }
    }
}
