using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataServicio.Migrations
{
    /// <inheritdoc />
    public partial class inicio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Persona",
                columns: table => new
                {
                    PersonaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ApePaterno = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    ApeMaterno = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    DNI = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    CUIT = table.Column<string>(type: "nvarchar(19)", maxLength: 19, nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persona", x => x.PersonaId);
                });

            migrationBuilder.CreateTable(
                name: "Producto",
                columns: table => new
                {
                    ProductoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RubroA = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RubroB = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RubroC = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Producto", x => x.ProductoId);
                });

            migrationBuilder.CreateTable(
                name: "RubroFerreteria",
                columns: table => new
                {
                    RubroFerreteriaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RubroFerreteria", x => x.RubroFerreteriaId);
                });

            migrationBuilder.CreateTable(
                name: "Direccion",
                columns: table => new
                {
                    DireccionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Calle = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Piso = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Depto = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PersonaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Direccion", x => x.DireccionId);
                    table.ForeignKey(
                        name: "FK_Direccion_Persona_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "Persona",
                        principalColumn: "PersonaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Email",
                columns: table => new
                {
                    EmailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(90)", maxLength: 90, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    PersonaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Email", x => x.EmailId);
                    table.ForeignKey(
                        name: "FK_Email_Persona_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "Persona",
                        principalColumn: "PersonaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Proveedor",
                columns: table => new
                {
                    ProveedorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    PersonaId = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedor", x => x.ProveedorId);
                    table.ForeignKey(
                        name: "FK_Proveedor_Persona_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "Persona",
                        principalColumn: "PersonaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Telefono",
                columns: table => new
                {
                    TelefonoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    PersonaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Telefono", x => x.TelefonoId);
                    table.ForeignKey(
                        name: "FK_Telefono_Persona_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "Persona",
                        principalColumn: "PersonaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoginUser = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    LoginPwd = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    PersonaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.UsuarioId);
                    table.ForeignKey(
                        name: "FK_Usuario_Persona_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "Persona",
                        principalColumn: "PersonaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ListaPrecioProveedor",
                columns: table => new
                {
                    ListaPrecioProveedorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FecIngreso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Comentario = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProveedorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListaPrecioProveedor", x => x.ListaPrecioProveedorId);
                    table.ForeignKey(
                        name: "FK_ListaPrecioProveedor_Proveedor_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedor",
                        principalColumn: "ProveedorId");
                });

            migrationBuilder.CreateTable(
                name: "RubroFerreteriaRelacion",
                columns: table => new
                {
                    ProveedoresProveedorId = table.Column<int>(type: "int", nullable: false),
                    RubrosRubroFerreteriaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RubroFerreteriaRelacion", x => new { x.ProveedoresProveedorId, x.RubrosRubroFerreteriaId });
                    table.ForeignKey(
                        name: "FK_RubroFerreteriaRelacion_Proveedor_ProveedoresProveedorId",
                        column: x => x.ProveedoresProveedorId,
                        principalTable: "Proveedor",
                        principalColumn: "ProveedorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RubroFerreteriaRelacion_RubroFerreteria_RubrosRubroFerreteriaId",
                        column: x => x.RubrosRubroFerreteriaId,
                        principalTable: "RubroFerreteria",
                        principalColumn: "RubroFerreteriaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LisPreProConfiguracion",
                columns: table => new
                {
                    LisPreProConfiguracionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaveLocal = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    ClaveProveedor = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    ListaPrecioProveedorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LisPreProConfiguracion", x => x.LisPreProConfiguracionId);
                    table.ForeignKey(
                        name: "FK_LisPreProConfiguracion_ListaPrecioProveedor_ListaPrecioProveedorId",
                        column: x => x.ListaPrecioProveedorId,
                        principalTable: "ListaPrecioProveedor",
                        principalColumn: "ListaPrecioProveedorId");
                });

            migrationBuilder.CreateTable(
                name: "LisPreProData",
                columns: table => new
                {
                    LisPreProDataId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion1 = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Descripcion2 = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Descripcion3 = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Descripcion4 = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Descripcion5 = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Descripcion6 = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Valor1 = table.Column<double>(type: "float", nullable: false),
                    Valor2 = table.Column<double>(type: "float", nullable: false),
                    Valor3 = table.Column<double>(type: "float", nullable: false),
                    ListaPrecioProveedorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LisPreProData", x => x.LisPreProDataId);
                    table.ForeignKey(
                        name: "FK_LisPreProData_ListaPrecioProveedor_ListaPrecioProveedorId",
                        column: x => x.ListaPrecioProveedorId,
                        principalTable: "ListaPrecioProveedor",
                        principalColumn: "ListaPrecioProveedorId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Direccion_PersonaId",
                table: "Direccion",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_Email_PersonaId",
                table: "Email",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_LisPreProConfiguracion_ListaPrecioProveedorId",
                table: "LisPreProConfiguracion",
                column: "ListaPrecioProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_LisPreProData_ListaPrecioProveedorId",
                table: "LisPreProData",
                column: "ListaPrecioProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_ListaPrecioProveedor_ProveedorId",
                table: "ListaPrecioProveedor",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedor_PersonaId",
                table: "Proveedor",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_RubroFerreteriaRelacion_RubrosRubroFerreteriaId",
                table: "RubroFerreteriaRelacion",
                column: "RubrosRubroFerreteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Telefono_PersonaId",
                table: "Telefono",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_PersonaId",
                table: "Usuario",
                column: "PersonaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Direccion");

            migrationBuilder.DropTable(
                name: "Email");

            migrationBuilder.DropTable(
                name: "LisPreProConfiguracion");

            migrationBuilder.DropTable(
                name: "LisPreProData");

            migrationBuilder.DropTable(
                name: "Producto");

            migrationBuilder.DropTable(
                name: "RubroFerreteriaRelacion");

            migrationBuilder.DropTable(
                name: "Telefono");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropTable(
                name: "ListaPrecioProveedor");

            migrationBuilder.DropTable(
                name: "RubroFerreteria");

            migrationBuilder.DropTable(
                name: "Proveedor");

            migrationBuilder.DropTable(
                name: "Persona");
        }
    }
}
