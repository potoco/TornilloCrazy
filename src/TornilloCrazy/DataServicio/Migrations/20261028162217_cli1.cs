using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataServicio.Migrations
{
    /// <inheritdoc />
    public partial class cli1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClienteTblClienteId",
                table: "UsuarioRol",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cliente",
                columns: table => new
                {
                    ClienteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    PersonaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cliente", x => x.ClienteId);
                    table.ForeignKey(
                        name: "FK_Cliente_Persona_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "Persona",
                        principalColumn: "PersonaId",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_PersonaId",
                table: "Cliente",
                column: "PersonaId");

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioRol_Cliente_ClienteTblClienteId",
                table: "UsuarioRol",
                column: "ClienteTblClienteId",
                principalTable: "Cliente",
                principalColumn: "ClienteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioRol_Cliente_ClienteTblClienteId",
                table: "UsuarioRol");

            migrationBuilder.DropTable(
                name: "Cliente");

            migrationBuilder.DropIndex(
                name: "IX_UsuarioRol_ClienteTblClienteId",
                table: "UsuarioRol");

            migrationBuilder.DropColumn(
                name: "ClienteTblClienteId",
                table: "UsuarioRol");
        }
    }
}
