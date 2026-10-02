using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Product.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261002120000_PromoteExistingAdministrators")]
public class PromoteExistingAdministrators : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql("UPDATE [User] SET [UserType] = 'SuperAdmin' WHERE [UserType] = 'Admin' AND [Id] IN (SELECT [Id] FROM [Administrators])");

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql("UPDATE [User] SET [UserType] = 'Admin' WHERE [UserType] = 'SuperAdmin' AND [Id] IN (SELECT [Id] FROM [Administrators])");
}
