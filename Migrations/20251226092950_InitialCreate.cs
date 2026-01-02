using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Futebol.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Partidas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SorteioId = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeCasaId = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeVisitanteId = table.Column<int>(type: "INTEGER", nullable: false),
                    GolsCasa = table.Column<int>(type: "INTEGER", nullable: true),
                    GolsVisitante = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partidas", x => x.Id);
                    table.CheckConstraint("CK_Partida_Times_Diferentes", "TimeCasaId <> TimeVisitanteId");
                });

            migrationBuilder.CreateTable(
                name: "Sorteios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizadorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    QuantidadeTimes = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sorteios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Times",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SorteioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Times", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Times_Sorteios_SorteioId",
                        column: x => x.SorteioId,
                        principalTable: "Sorteios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Organizadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organizadores_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Jogadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizadorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Posicao = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Observacoes = table.Column<string>(type: "TEXT", nullable: true),
                    Destaque = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    Peso = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jogadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jogadores_Organizadores_OrganizadorId",
                        column: x => x.OrganizadorId,
                        principalTable: "Organizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeJogadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TimeId = table.Column<int>(type: "INTEGER", nullable: false),
                    JogadorId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeJogadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeJogadores_Jogadores_JogadorId",
                        column: x => x.JogadorId,
                        principalTable: "Jogadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimeJogadores_Times_TimeId",
                        column: x => x.TimeId,
                        principalTable: "Times",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jogadores_OrganizadorId",
                table: "Jogadores",
                column: "OrganizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizadores_UserId",
                table: "Organizadores",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeJogadores_JogadorId",
                table: "TimeJogadores",
                column: "JogadorId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeJogadores_TimeId_JogadorId",
                table: "TimeJogadores",
                columns: new[] { "TimeId", "JogadorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Times_SorteioId",
                table: "Times",
                column: "SorteioId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Partidas");

            migrationBuilder.DropTable(
                name: "TimeJogadores");

            migrationBuilder.DropTable(
                name: "Jogadores");

            migrationBuilder.DropTable(
                name: "Times");

            migrationBuilder.DropTable(
                name: "Organizadores");

            migrationBuilder.DropTable(
                name: "Sorteios");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
