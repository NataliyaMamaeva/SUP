using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication3.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterial_Color_ItemType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Project__ClientI__52593CB8",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK__Project__Deliver__534D60F1",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK__Project__Deliver__5441852A",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK__Project__Employe__5165187F",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK__ProjectFi__Proje__6C190EBB",
                table: "ProjectFile");

            migrationBuilder.DropPrimaryKey(
                name: "PK__ProjectF__2035851587E23999",
                table: "ProjectFile");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Project__761ABEF056D884C6",
                table: "Project");

            migrationBuilder.RenameColumn(
                name: "LayoutId",
                table: "ProjectFile",
                newName: "FileId");

            migrationBuilder.DropColumn(
               name: "FileId",
               table: "ProjectFile"
               );

           migrationBuilder.AddColumn<int>(
                name: "FileId",
                table: "ProjectFile",
                type: "int",
                nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<decimal>(
                name: "PaymentTotal",
                table: "Project",
                type: "decimal(6,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,2)");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "PaymentDate",
                table: "Project",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<decimal>(
                name: "AdvanceRate",
                table: "Project",
                type: "decimal(3,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,2)");

            migrationBuilder.AddPrimaryKey(
                name: "PK__ProjectF__6F0F98BF593087A7",
                table: "ProjectFile",
                column: "FileId");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Project__761ABEF006CB3F38",
                table: "Project",
                column: "ProjectId");

            migrationBuilder.CreateTable(
                name: "Color",
                columns: table => new
                {
                    ColorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColorName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Color__8DA7674D668C3A7C", x => x.ColorId);
                });

            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ItemType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sketch = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<int>(type: "int", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    ItemDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Materials = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Colors = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deadline = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Item__727E838BFA193C84", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK__Item__ProjectId__5441852A",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "ProjectId");
                    table.ForeignKey(
                        name: "FK__Item__Sketch__5535A963",
                        column: x => x.Sketch,
                        principalTable: "ProjectFile",
                        principalColumn: "FileId");
                });

            migrationBuilder.CreateTable(
                name: "ItemType",
                columns: table => new
                {
                    ItemTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemTypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ItemType__F51540FBE00BE195", x => x.ItemTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Material",
                columns: table => new
                {
                    MaterialId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaterialName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Material__C50610F7B32E7551", x => x.MaterialId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Item_ProjectId",
                table: "Item",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_Sketch",
                table: "Item",
                column: "Sketch");

            migrationBuilder.AddForeignKey(
                name: "FK__Project__ClientI__4222D4EF",
                table: "Project",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK__Project__Deliver__4316F928",
                table: "Project",
                column: "DeliveryToAddress",
                principalTable: "DeliveryAddress",
                principalColumn: "AddressId");

            migrationBuilder.AddForeignKey(
                name: "FK__Project__Deliver__440B1D61",
                table: "Project",
                column: "DeliveryToContact",
                principalTable: "Contact",
                principalColumn: "ContactId");

            migrationBuilder.AddForeignKey(
                name: "FK__Project__Employe__412EB0B6",
                table: "Project",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK__ProjectFi__Proje__47DBAE45",
                table: "ProjectFile",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Project__ClientI__4222D4EF",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK__Project__Deliver__4316F928",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK__Project__Deliver__440B1D61",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK__Project__Employe__412EB0B6",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK__ProjectFi__Proje__47DBAE45",
                table: "ProjectFile");

            migrationBuilder.DropTable(
                name: "Color");

            migrationBuilder.DropTable(
                name: "Item");

            migrationBuilder.DropTable(
                name: "ItemType");

            migrationBuilder.DropTable(
                name: "Material");

            migrationBuilder.DropPrimaryKey(
                name: "PK__ProjectF__6F0F98BF593087A7",
                table: "ProjectFile");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Project__761ABEF006CB3F38",
                table: "Project");

            migrationBuilder.RenameColumn(
                name: "FileId",
                table: "ProjectFile",
                newName: "LayoutId");

            migrationBuilder.AlterColumn<int>(
                name: "LayoutId",
                table: "ProjectFile",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<decimal>(
                name: "PaymentTotal",
                table: "Project",
                type: "decimal(6,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "PaymentDate",
                table: "Project",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AdvanceRate",
                table: "Project",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,2)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK__ProjectF__2035851587E23999",
                table: "ProjectFile",
                column: "LayoutId");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Project__761ABEF056D884C6",
                table: "Project",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK__Project__ClientI__52593CB8",
                table: "Project",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK__Project__Deliver__534D60F1",
                table: "Project",
                column: "DeliveryToAddress",
                principalTable: "DeliveryAddress",
                principalColumn: "AddressId");

            migrationBuilder.AddForeignKey(
                name: "FK__Project__Deliver__5441852A",
                table: "Project",
                column: "DeliveryToContact",
                principalTable: "Contact",
                principalColumn: "ContactId");

            migrationBuilder.AddForeignKey(
                name: "FK__Project__Employe__5165187F",
                table: "Project",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK__ProjectFi__Proje__6C190EBB",
                table: "ProjectFile",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "ProjectId");
        }
    }
}
