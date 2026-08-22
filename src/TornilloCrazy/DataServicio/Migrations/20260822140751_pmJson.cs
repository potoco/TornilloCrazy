using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataServicio.Migrations
{
    /// <inheritdoc />
    public partial class pmJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "ProveedorMaestro");

            migrationBuilder.AddColumn<string>(
                name: "Descripcion1",
                table: "ProveedorMaestro",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstadoRevisionIA",
                table: "ProveedorMaestro",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRevisionIA",
                table: "ProveedorMaestro",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JsonRaw",
                table: "ProveedorMaestro",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreCanonico",
                table: "ProveedorMaestro",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descripcion1",
                table: "ProveedorMaestro");

            migrationBuilder.DropColumn(
                name: "EstadoRevisionIA",
                table: "ProveedorMaestro");

            migrationBuilder.DropColumn(
                name: "FechaRevisionIA",
                table: "ProveedorMaestro");

            migrationBuilder.DropColumn(
                name: "JsonRaw",
                table: "ProveedorMaestro");

            migrationBuilder.DropColumn(
                name: "NombreCanonico",
                table: "ProveedorMaestro");

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "ProveedorMaestro",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
