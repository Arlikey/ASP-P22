using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP_P22.Migrations
{
    /// <inheritdoc />
    public partial class UserSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                schema: "site",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slug",
                schema: "site",
                table: "Users");
        }
    }
}
