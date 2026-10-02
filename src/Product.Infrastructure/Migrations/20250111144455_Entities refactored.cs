using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Product.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Entitiesrefactored : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invites_Administrators_AdminId",
                table: "Invites");

            migrationBuilder.DropForeignKey(
                name: "FK_Invites_User_UserId",
                table: "Invites");

            migrationBuilder.RenameColumn(
                name: "AdminId",
                table: "Invites",
                newName: "SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_Invites_AdminId",
                table: "Invites",
                newName: "IX_Invites_SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invites_User_SenderId",
                table: "Invites",
                column: "SenderId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invites_User_UserId",
                table: "Invites",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invites_User_SenderId",
                table: "Invites");

            migrationBuilder.DropForeignKey(
                name: "FK_Invites_User_UserId",
                table: "Invites");

            migrationBuilder.RenameColumn(
                name: "SenderId",
                table: "Invites",
                newName: "AdminId");

            migrationBuilder.RenameIndex(
                name: "IX_Invites_SenderId",
                table: "Invites",
                newName: "IX_Invites_AdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invites_Administrators_AdminId",
                table: "Invites",
                column: "AdminId",
                principalTable: "Administrators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invites_User_UserId",
                table: "Invites",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
