using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataServicio.Migrations
{
    /// <inheritdoc />
    public partial class asociarProductoListaPrecio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductoId",
                table: "LisPreProData",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LisPreProData_ProductoId",
                table: "LisPreProData",
                column: "ProductoId");

            migrationBuilder.AddForeignKey(
                name: "FK_LisPreProData_Producto_ProductoId",
                table: "LisPreProData",
                column: "ProductoId",
                principalTable: "Producto",
                principalColumn: "ProductoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LisPreProData_Producto_ProductoId",
                table: "LisPreProData");

            migrationBuilder.DropIndex(
                name: "IX_LisPreProData_ProductoId",
                table: "LisPreProData");

            migrationBuilder.DropColumn(
                name: "ProductoId",
                table: "LisPreProData");
        }
    }
}
