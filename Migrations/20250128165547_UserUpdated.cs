using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP_P22.Migrations
{
    /// <inheritdoc />
    public partial class UserUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Phone",
                schema: "site",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                schema: "site",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkPosition",
                schema: "site",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Phone",
                schema: "site",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                schema: "site",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WorkPosition",
                schema: "site",
                table: "Users");
        }
    }
}
