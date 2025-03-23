using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class NotesEmailConfirmations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchiveAccountId",
                table: "Project");

            migrationBuilder.RenameColumn(
                name: "Journal",
                table: "Project",
                newName: "LayoutsRequired");

            migrationBuilder.AddColumn<int>(
                name: "JournalNoteId",
                table: "ProjectFile",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArchiveAccountEmail",
                table: "Project",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDocumentsComleted",
                table: "Project",
                type: "bit",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Contact",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Contact",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "EmailConfirmations",
                columns: table => new
                {
                    EmailConfirmationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailConfirmations", x => x.EmailConfirmationId);
                    table.ForeignKey(
                        name: "FK_EmailConfirmations_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JournalTopics",
                columns: table => new
                {
                    JournalTopicId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalTopicName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalTopics", x => x.JournalTopicId);
                });

            migrationBuilder.CreateTable(
                name: "JournalNotes",
                columns: table => new
                {
                    JournalNoteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalNoteDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JournalTopicName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JournalTopicId = table.Column<int>(type: "int", nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalNotes", x => x.JournalNoteId);
                    table.ForeignKey(
                        name: "FK_JournalNotes_JournalTopics_JournalTopicId",
                        column: x => x.JournalTopicId,
                        principalTable: "JournalTopics",
                        principalColumn: "JournalTopicId");
                    table.ForeignKey(
                        name: "FK_JournalNotes_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "ProjectId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectFile_JournalNoteId",
                table: "ProjectFile",
                column: "JournalNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailConfirmations_EmployeeId",
                table: "EmailConfirmations",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalNotes_JournalTopicId",
                table: "JournalNotes",
                column: "JournalTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalNotes_ProjectId",
                table: "JournalNotes",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectFile_JournalNotes_JournalNoteId",
                table: "ProjectFile",
                column: "JournalNoteId",
                principalTable: "JournalNotes",
                principalColumn: "JournalNoteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectFile_JournalNotes_JournalNoteId",
                table: "ProjectFile");

            migrationBuilder.DropTable(
                name: "EmailConfirmations");

            migrationBuilder.DropTable(
                name: "JournalNotes");

            migrationBuilder.DropTable(
                name: "JournalTopics");

            migrationBuilder.DropIndex(
                name: "IX_ProjectFile_JournalNoteId",
                table: "ProjectFile");

            migrationBuilder.DropColumn(
                name: "JournalNoteId",
                table: "ProjectFile");

            migrationBuilder.DropColumn(
                name: "ArchiveAccountEmail",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "IsDocumentsComleted",
                table: "Project");

            migrationBuilder.RenameColumn(
                name: "LayoutsRequired",
                table: "Project",
                newName: "Journal");

            migrationBuilder.AddColumn<int>(
                name: "ArchiveAccountId",
                table: "Project",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Contact",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Contact",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
