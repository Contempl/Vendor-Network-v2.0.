# .NET 10 development setup

All eight projects target .NET 10. Install SDK 10.0.401 selected by `global.json`;
later patches in the same feature band are allowed. CI reads that file. Docker
uses SDK 10.0.401 and ASP.NET Core runtime 10.0.12.

```powershell
dotnet --version
dotnet restore src/Project.sln
dotnet tool restore
dotnet build src/Project.sln --configuration Release --no-restore
dotnet test src/Project.sln --configuration Release --no-build
```

EF Core and the local `dotnet-ef` tool use 10.0.12; Npgsql uses 10.0.3. Integration
tests require Docker Engine and start disposable PostgreSQL and SQL Server
containers. They verify HTTP authorization, real JWT header/cookie authentication,
Swagger, PostgreSQL migrations and SQL Server data transfer.

The earlier .NET upgrade added a SQL Server migration limiting the business
discriminator to `nvarchar(8)`. It is retained in the SQL Server migration archive.
The application now uses PostgreSQL; see [the migration guide](postgresql-migration.md)
for current connection strings, schema commands, Docker setup and data transfer.

The API container listens on port 80 through `ASPNETCORE_HTTP_PORTS`. Swagger keeps
`/swagger/v1/swagger.json` and its `Bearer` security definition.
