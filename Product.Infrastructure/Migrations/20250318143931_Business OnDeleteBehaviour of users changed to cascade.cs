using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Product.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BusinessOnDeleteBehaviourofuserschangedtocascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OperatorUsers_Businesses_OperatorId",
                table: "OperatorUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_VendorUsers_Businesses_VendorId",
                table: "VendorUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "VendorsFacilityServices",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AddForeignKey(
                name: "FK_OperatorUsers_Businesses_OperatorId",
                table: "OperatorUsers",
                column: "OperatorId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VendorUsers_Businesses_VendorId",
                table: "VendorUsers",
                column: "VendorId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OperatorUsers_Businesses_OperatorId",
                table: "OperatorUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_VendorUsers_Businesses_VendorId",
                table: "VendorUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "VendorsFacilityServices",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AddForeignKey(
                name: "FK_OperatorUsers_Businesses_OperatorId",
                table: "OperatorUsers",
                column: "OperatorId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VendorUsers_Businesses_VendorId",
                table: "VendorUsers",
                column: "VendorId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
