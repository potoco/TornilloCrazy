using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataServicio.Migrations
{
    /// <inheritdoc />
    public partial class masprop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaSeleccionProducto",
                table: "ProveedorMaestro",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JsonRaw",
                table: "Producto",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaSeleccionProducto",
                table: "ProveedorMaestro");

            migrationBuilder.DropColumn(
                name: "JsonRaw",
                table: "Producto");
        }
    }
}
