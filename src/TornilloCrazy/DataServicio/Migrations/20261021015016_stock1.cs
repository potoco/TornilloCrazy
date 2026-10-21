using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DataServicio.Migrations
{
    /// <inheritdoc />
    public partial class stock1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockControl",
                columns: table => new
                {
                    StockControlId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    CodigoAlteracion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockControl", x => x.StockControlId);
                });

            migrationBuilder.CreateTable(
                name: "TipoAlteracionStock",
                columns: table => new
                {
                    TipoAlteracionStockId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodigoAlteracion = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoAlteracionStock", x => x.TipoAlteracionStockId);
                });

            migrationBuilder.InsertData(
                table: "TipoAlteracionStock",
                columns: new[] { "TipoAlteracionStockId", "CodigoAlteracion", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, "CS", "Es el evento que marca un punto de partida", "Control de Stock" },
                    { 2, "II", "Es el evento que marca un ingreso de stock", "Ingreso de stock" },
                    { 3, "EE", "Es el evento que marca EGRESO o SALIDA del stock", "Salida de stock" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockControl");

            migrationBuilder.DropTable(
                name: "TipoAlteracionStock");
        }
    }
}
