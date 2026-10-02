# .NET 10 development setup

All projects, including the seeder and both test suites, target .NET 10. Install
the .NET SDK selected by `global.json` (10.0.401, with later patches in the same
feature band allowed). CI reads that file; the Docker build uses SDK 10.0.401 and
ASP.NET Core runtime 10.0.12.

```powershell
dotnet --version
dotnet restore src/Project.sln
dotnet tool restore
dotnet build src/Project.sln --configuration Release --no-restore
dotnet test src/Product.Tests/Product.Tests.csproj --configuration Release --no-build
dotnet test src/Product.IntegrationTests/Product.IntegrationTests.csproj --configuration Release --no-build
```

The integration suite needs Docker Engine running. HTTP tests start PostgreSQL
containers; the migration test starts a separate SQL Server 2022 container. Test
containers are disposable and do not use the application's local database.
The account tests also exercise the real JWT handler through both the bearer
header and the existing authentication cookie, and validate Swagger output.

## EF Core migrations

EF Core, its SQL Server provider, and the local `dotnet-ef` tool use version
10.0.12. The PostgreSQL provider uses a compatible 10.x release. The API and
seeder still use SQL Server; the PostgreSQL database in the root Compose file
does not supply their database.

EF Core 8 introduced a length limit for string discriminators. The
`UpgradeToEfCore10` migration changes `Businesses.BusinessType` from
`nvarchar(max)` to `nvarchar(8)`, sufficient for the existing discriminator
values (`Business`, `Operator`, and `Vendor`). Its rollback restores the old
column type. The SQL Server integration test applies the historical migrations,
seeds businesses and an administrator, then verifies that upgrade and rollback
preserve their data.

Run EF commands from the API directory because the existing design-time factory
loads `appsettings.json` relative to the working directory:

```powershell
Push-Location src/Product.WebApi
dotnet ef migrations has-pending-model-changes --project ../Product.Infrastructure/Product.Infrastructure.csproj --startup-project Product.WebApi.csproj --configuration Release --no-build
Pop-Location
```

CI runs the same snapshot check to detect model changes without a migration.
Before applying migrations locally, set `ConnectionStrings__DefaultConnection`
to the intended local SQL Server database. Then, from `src/Product.WebApi`, run:

```powershell
dotnet ef database update --project ../Product.Infrastructure/Product.Infrastructure.csproj --startup-project Product.WebApi.csproj --configuration Release --no-build
```

The Docker API keeps listening on container port 80 through
`ASPNETCORE_HTTP_PORTS`, preserving the existing Compose port mapping. Swagger
keeps its `/swagger/v1/swagger.json` URL and the `Bearer` security definition.
