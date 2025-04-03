using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ASP_P22.Migrations
{
    /// <inheritdoc />
    public partial class Token : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RoleId",
                schema: "site",
                table: "Accesses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuthTokens",
                schema: "site",
                columns: table => new
                {
                    Jti = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Iss = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sub = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Aud = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Iat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Exp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Nbf = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthTokens", x => x.Jti);
                    table.ForeignKey(
                        name: "FK_AuthTokens_Accesses_Sub",
                        column: x => x.Sub,
                        principalSchema: "site",
                        principalTable: "Accesses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                schema: "site",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CanCreate = table.Column<bool>(type: "bit", nullable: false),
                    CanRead = table.Column<bool>(type: "bit", nullable: false),
                    CanUpdate = table.Column<bool>(type: "bit", nullable: false),
                    CanDelete = table.Column<bool>(type: "bit", nullable: false),
                    IsEmployee = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "site",
                table: "UserRoles",
                columns: new[] { "Id", "CanCreate", "CanDelete", "CanRead", "CanUpdate", "Description", "IsEmployee", "Name" },
                values: new object[,]
                {
                    { new Guid("d1a3d3a4-3a3d-4d1a-3a3d-4d1a3d3a4d1a"), true, true, true, true, "Адміністратор", true, "Admin" },
                    { new Guid("d1a3d3a4-3a3d-4d1a-3a3d-4d1a3d3a4d2a"), false, false, false, false, "Користувач", false, "User" },
                    { new Guid("d1a3d3a4-3a3d-4d1a-3a3d-4d1a3d3a4d3a"), false, false, false, false, "Працівник", true, "Employee" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accesses_RoleId",
                schema: "site",
                table: "Accesses",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthTokens_Sub",
                schema: "site",
                table: "AuthTokens",
                column: "Sub");

            migrationBuilder.AddForeignKey(
                name: "FK_Accesses_UserRoles_RoleId",
                schema: "site",
                table: "Accesses",
                column: "RoleId",
                principalSchema: "site",
                principalTable: "UserRoles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accesses_UserRoles_RoleId",
                schema: "site",
                table: "Accesses");

            migrationBuilder.DropTable(
                name: "AuthTokens",
                schema: "site");

            migrationBuilder.DropTable(
                name: "UserRoles",
                schema: "site");

            migrationBuilder.DropIndex(
                name: "IX_Accesses_RoleId",
                schema: "site",
                table: "Accesses");

            migrationBuilder.DropColumn(
                name: "RoleId",
                schema: "site",
                table: "Accesses");
        }
    }
}
