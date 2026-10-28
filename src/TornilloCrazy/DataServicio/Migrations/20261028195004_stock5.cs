using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataServicio.Migrations
{
    /// <inheritdoc />
    public partial class stock5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockControlId",
                table: "Venta");

            migrationBuilder.CreateTable(
                name: "VentaStock",
                columns: table => new
                {
                    VentaId = table.Column<int>(type: "int", nullable: false),
                    StockControlId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentaStock", x => new { x.VentaId, x.StockControlId });
                    table.ForeignKey(
                        name: "FK_VentaStock_StockControl_StockControlId",
                        column: x => x.StockControlId,
                        principalTable: "StockControl",
                        principalColumn: "StockControlId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VentaStock_Venta_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Venta",
                        principalColumn: "VentaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VentaStock_StockControlId",
                table: "VentaStock",
                column: "StockControlId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VentaStock");

            migrationBuilder.AddColumn<int>(
                name: "StockControlId",
                table: "Venta",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
