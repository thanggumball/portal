using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class FixUserNameUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Users_UserName",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "UX_Users_RoleId_UserName",
                table: "Users",
                columns: new[] { "RoleId", "UserName" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Users_RoleId_UserName",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "UX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true,
                filter: "[IsDeleted] = 0");
        }
    }
}
