using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataServicio.Migrations
{
    /// <inheritdoc />
    public partial class camposvec : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AtributosJson",
                table: "Producto",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SinonimosJson",
                table: "Producto",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TextoVectorial",
                table: "Producto",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<SqlVector<float>>(
                name: "VectorEmbedding",
                table: "Producto",
                type: "vector(1536)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AtributosJson",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "SinonimosJson",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "TextoVectorial",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "VectorEmbedding",
                table: "Producto");
        }
    }
}
