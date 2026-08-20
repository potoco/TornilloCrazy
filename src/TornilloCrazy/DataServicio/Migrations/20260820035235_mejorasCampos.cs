using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataServicio.Migrations
{
    /// <inheritdoc />
    public partial class mejorasCampos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NombreArchivo",
                table: "ListaPrecioProveedor",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreArchivoOriginal",
                table: "ListaPrecioProveedor",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion6",
                table: "LisPreProData",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion5",
                table: "LisPreProData",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion4",
                table: "LisPreProData",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion3",
                table: "LisPreProData",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion2",
                table: "LisPreProData",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion1",
                table: "LisPreProData",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LineaCruda",
                table: "LisPreProData",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NombreArchivo",
                table: "ListaPrecioProveedor");

            migrationBuilder.DropColumn(
                name: "NombreArchivoOriginal",
                table: "ListaPrecioProveedor");

            migrationBuilder.DropColumn(
                name: "LineaCruda",
                table: "LisPreProData");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion6",
                table: "LisPreProData",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(254)",
                oldMaxLength: 254,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion5",
                table: "LisPreProData",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(254)",
                oldMaxLength: 254,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion4",
                table: "LisPreProData",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(254)",
                oldMaxLength: 254,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion3",
                table: "LisPreProData",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(254)",
                oldMaxLength: 254,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion2",
                table: "LisPreProData",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(254)",
                oldMaxLength: 254,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion1",
                table: "LisPreProData",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(254)",
                oldMaxLength: 254,
                oldNullable: true);
        }
    }
}
