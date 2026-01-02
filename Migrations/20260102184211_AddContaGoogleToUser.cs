using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Futebol.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddContaGoogleToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ContaGoogle",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContaGoogle",
                table: "Users");
        }
    }
}
