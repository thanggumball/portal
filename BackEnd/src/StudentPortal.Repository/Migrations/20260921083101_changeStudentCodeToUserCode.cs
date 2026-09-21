using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class changeStudentCodeToUserCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_RoleId_CreatedAt",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "UX_Users_StudentCode",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "StudentCode",
                table: "Users",
                newName: "UserCode");

            migrationBuilder.CreateTable(
                name: "AccountSequences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountType = table.Column<string>(type: "varchar(20)", nullable: false),
                    NextNumber = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountSequences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId_CreatedAt",
                table: "Users",
                columns: new[] { "RoleId", "CreatedAt" },
                descending: new[] { false, true },
                filter: "[IsDeleted] = 0")
                .Annotation("SqlServer:Include", new[] { "Email", "FullName", "UserCode", "LastLoginAt" });

            migrationBuilder.CreateIndex(
                name: "UX_AccountSequences_AccountType",
                table: "AccountSequences",
                column: "AccountType",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountSequences");

            migrationBuilder.DropIndex(
                name: "IX_Users_RoleId_CreatedAt",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "UserCode",
                table: "Users",
                newName: "StudentCode");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId_CreatedAt",
                table: "Users",
                columns: new[] { "RoleId", "CreatedAt" },
                descending: new[] { false, true },
                filter: "[IsDeleted] = 0")
                .Annotation("SqlServer:Include", new[] { "Email", "FullName", "StudentCode", "LastLoginAt" });

            migrationBuilder.CreateIndex(
                name: "UX_Users_StudentCode",
                table: "Users",
                column: "StudentCode",
                unique: true,
                filter: "[IsDeleted] = 0 AND [StudentCode] IS NOT NULL");
        }
    }
}
