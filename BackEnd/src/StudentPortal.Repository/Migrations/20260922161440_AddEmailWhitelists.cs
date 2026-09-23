using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailWhitelists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "EmailWhitelists");

            migrationBuilder.RenameColumn(
                name: "Domain",
                table: "EmailWhitelists",
                newName: "Email");

            migrationBuilder.RenameIndex(
                name: "UX_EmailWhitelists_Domain",
                table: "EmailWhitelists",
                newName: "UX_EmailWhitelists_Email");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "EmailWhitelists",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "EmailWhitelists",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "RoleId",
                table: "EmailWhitelists",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UsedAt",
                table: "EmailWhitelists",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailWhitelists_CreatedBy",
                table: "EmailWhitelists",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EmailWhitelists_RoleId",
                table: "EmailWhitelists",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailWhitelists_Roles_RoleId",
                table: "EmailWhitelists",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailWhitelists_Users_CreatedBy",
                table: "EmailWhitelists",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailWhitelists_Roles_RoleId",
                table: "EmailWhitelists");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailWhitelists_Users_CreatedBy",
                table: "EmailWhitelists");

            migrationBuilder.DropIndex(
                name: "IX_EmailWhitelists_CreatedBy",
                table: "EmailWhitelists");

            migrationBuilder.DropIndex(
                name: "IX_EmailWhitelists_RoleId",
                table: "EmailWhitelists");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "EmailWhitelists");

            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "EmailWhitelists");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "EmailWhitelists");

            migrationBuilder.DropColumn(
                name: "UsedAt",
                table: "EmailWhitelists");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "EmailWhitelists",
                newName: "Domain");

            migrationBuilder.RenameIndex(
                name: "UX_EmailWhitelists_Email",
                table: "EmailWhitelists",
                newName: "UX_EmailWhitelists_Domain");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "EmailWhitelists",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }
    }
}
