using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataServicio.Migrations
{
    /// <inheritdoc />
    public partial class prdmejor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RubroA",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "RubroB",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "RubroC",
                table: "Producto");

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Producto",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioUnitario",
                table: "Producto",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Rubros",
                table: "Producto",
                type: "nvarchar(240)",
                maxLength: 240,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sinonimos",
                table: "Producto",
                type: "nvarchar(240)",
                maxLength: 240,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrecioUnitario",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "Rubros",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "Sinonimos",
                table: "Producto");

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Producto",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RubroA",
                table: "Producto",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RubroB",
                table: "Producto",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RubroC",
                table: "Producto",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
