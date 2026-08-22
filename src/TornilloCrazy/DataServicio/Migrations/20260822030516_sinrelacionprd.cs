using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataServicio.Migrations
{
    /// <inheritdoc />
    public partial class sinrelacionprd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProveedorMaestro_Producto_ProductoId",
                table: "ProveedorMaestro");

            migrationBuilder.AlterColumn<int>(
                name: "ProductoId",
                table: "ProveedorMaestro",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_ProveedorMaestro_Producto_ProductoId",
                table: "ProveedorMaestro",
                column: "ProductoId",
                principalTable: "Producto",
                principalColumn: "ProductoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProveedorMaestro_Producto_ProductoId",
                table: "ProveedorMaestro");

            migrationBuilder.AlterColumn<int>(
                name: "ProductoId",
                table: "ProveedorMaestro",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProveedorMaestro_Producto_ProductoId",
                table: "ProveedorMaestro",
                column: "ProductoId",
                principalTable: "Producto",
                principalColumn: "ProductoId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
