using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataServicio.Migrations
{
    /// <inheritdoc />
    public partial class provMaestro2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProveedorMaestro",
                columns: table => new
                {
                    ProveedorMaestroId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AtributosJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SinonimosJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsosJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RubrosJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TextoVectorial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VectorEmbedding = table.Column<SqlVector<float>>(type: "vector(1536)", nullable: true),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProveedorMaestro", x => x.ProveedorMaestroId);
                    table.ForeignKey(
                        name: "FK_ProveedorMaestro_Producto_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Producto",
                        principalColumn: "ProductoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProveedorMaestro_Proveedor_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedor",
                        principalColumn: "ProveedorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProveedorMaestro_ProductoId",
                table: "ProveedorMaestro",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProveedorMaestro_ProveedorId",
                table: "ProveedorMaestro",
                column: "ProveedorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProveedorMaestro");
        }
    }
}
