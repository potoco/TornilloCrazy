using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataServicio.Migrations
{
    /// <inheritdoc />
    public partial class fecRevIa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstadoRevisionIA",
                table: "LisPreProData",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRevisionIA",
                table: "LisPreProData",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstadoRevisionIA",
                table: "LisPreProData");

            migrationBuilder.DropColumn(
                name: "FechaRevisionIA",
                table: "LisPreProData");
        }
    }
}
