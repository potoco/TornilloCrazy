using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataServicio.Migrations
{
    /// <inheritdoc />
    public partial class dtoper : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioRol_Cliente_ClienteTblClienteId",
                table: "UsuarioRol");

            migrationBuilder.DropIndex(
                name: "IX_UsuarioRol_ClienteTblClienteId",
                table: "UsuarioRol");

            migrationBuilder.DropColumn(
                name: "ClienteTblClienteId",
                table: "UsuarioRol");

            migrationBuilder.AddColumn<decimal>(
                name: "DescuentoPorcentaje",
                table: "Cliente",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescuentoPorcentaje",
                table: "Cliente");

            migrationBuilder.AddColumn<int>(
                name: "ClienteTblClienteId",
                table: "UsuarioRol",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "UsuarioRol",
                keyColumns: new[] { "RolId", "UsuarioId" },
                keyValues: new object[] { 1, 1 },
                column: "ClienteTblClienteId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioRol_ClienteTblClienteId",
                table: "UsuarioRol",
                column: "ClienteTblClienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioRol_Cliente_ClienteTblClienteId",
                table: "UsuarioRol",
                column: "ClienteTblClienteId",
                principalTable: "Cliente",
                principalColumn: "ClienteId");
        }
    }
}
