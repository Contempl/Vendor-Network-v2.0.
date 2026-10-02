using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Product.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Embeddedinterceptors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invites_User_UserId",
                table: "Invites");

            migrationBuilder.DropIndex(
                name: "IX_Invites_UserId",
                table: "Invites");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Invites",
                newName: "UpdatedBy");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "VendorsFacilityServices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "VendorsFacilityServices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "VendorsFacilityServices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "VendorsFacilityServices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "VendorsFacilities",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "VendorsFacilities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "VendorsFacilities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "VendorsFacilities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "User",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "User",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "User",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Invites",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InvitedUserId",
                table: "Invites",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Invites",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Industries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Industries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Industries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "Industries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Businesses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Businesses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Businesses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "Businesses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invites_InvitedUserId",
                table: "Invites",
                column: "InvitedUserId",
                unique: true,
                filter: "[InvitedUserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Invites_User_InvitedUserId",
                table: "Invites",
                column: "InvitedUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invites_User_InvitedUserId",
                table: "Invites");

            migrationBuilder.DropIndex(
                name: "IX_Invites_InvitedUserId",
                table: "Invites");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "VendorsFacilityServices");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "VendorsFacilityServices");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "VendorsFacilityServices");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "VendorsFacilityServices");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "VendorsFacilities");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "VendorsFacilities");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "VendorsFacilities");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "VendorsFacilities");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "User");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "User");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "User");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "User");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Invites");

            migrationBuilder.DropColumn(
                name: "InvitedUserId",
                table: "Invites");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Invites");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Industries");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Industries");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Industries");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Industries");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Businesses");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Invites",
                newName: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Invites_UserId",
                table: "Invites",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Invites_User_UserId",
                table: "Invites",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
