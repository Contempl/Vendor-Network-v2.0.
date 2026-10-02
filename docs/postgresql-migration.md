# PostgreSQL setup and SQL Server data transfer

The API, design-time factory, seeder and HTTP integration tests now use PostgreSQL
16 with Npgsql/EF Core 10. SQL Server is used only by the one-time transfer tool
and its integration tests. Historical SQL Server migrations are archived under
`src/Product.Infrastructure/Migrations/LegacySqlServer` and excluded from compilation.
Active migrations and the snapshot live under `Migrations/PostgreSql`.

## Local Docker setup

Install the .NET 10 SDK selected by `global.json`, and start Docker Engine.
Copy `.env.example` to `.env` and replace its password and JWT secret with local
values. Never commit `.env` or database backups. PostgreSQL credentials initialize
a new volume only; editing them does not change an existing database user's password.

```powershell
docker compose up -d --build api
```

Compose waits for healthy PostgreSQL and Redis, runs the migration image once,
then starts the API. The migration container exits successfully when done. The
API is at http://localhost:8080; Swagger is at http://localhost:8080/swagger in
Development. This Compose file defaults to Development for local use. Set
`ASPNETCORE_ENVIRONMENT=Production` for deployment.

For the client, set `NEXT_PUBLIC_API_URL=http://localhost:8080` in
`client/.env.local`, or set `API_HOST_PORT=5227` for the client's existing default.
Optional `POSTGRES_HOST_PORT` and `REDIS_HOST_PORT` override 5432 and 6379.
Container-to-container connections always use `db:5432` and `redis:6379`.

For a **fresh** database, create the initial SuperAdmin explicitly:

```powershell
# Set SEED_ADMIN_EMAIL and SEED_ADMIN_PASSWORD in your private .env first.
docker compose --profile seed run --rm seeder
```

Do not seed before importing an existing SQL Server database: the importer
requires an empty target. Seeding is idempotent when a SuperAdmin already exists.

## EF tooling and running the API on the host

```powershell
dotnet restore src/Project.sln
dotnet tool restore
dotnet build src/Project.sln --configuration Release
# Set this to your actual PostgreSQL connection string, using the published host port.
$env:ConnectionStrings__DefaultConnection = 'Host=localhost;Port=5432;Database=vendor_network;Username=vendor;Password=YOUR_LOCAL_PASSWORD'
dotnet run --project src/Product.Seeder -- --migrate-only
# Configure RedisSettings__Url, RedisSettings__InstanceName and JwtOptions__Secret
# for your local environment before starting the API.
dotnet run --project src/Product.WebApi --launch-profile http
```

The API itself does not apply migrations at startup. Apply them as a deployment
step before accepting requests. The console `--migrate-only` mode neither creates
accounts nor requires seed credentials.

Run EF commands from `src/Product.WebApi`. The factory accepts environment
overrides and does not require a real connection to scaffold or check migrations.

```powershell
dotnet ef migrations has-pending-model-changes --project ../Product.Infrastructure --startup-project . --configuration Release --no-build
dotnet ef migrations add YourChange --project ../Product.Infrastructure --startup-project . --output-dir Migrations/PostgreSql
dotnet ef database update --project ../Product.Infrastructure --startup-project .
```

Do not apply the PostgreSQL initial migration to SQL Server or an existing schema
created by `EnsureCreated`. A database with pre-existing tables needs its schema
reviewed and baselined explicitly; the normal path is a fresh PostgreSQL database.

## Transfer an existing SQL Server database

1. Stop writes to the SQL Server application and take a verified full database
   backup (`COPY_ONLY`, `CHECKSUM`, then `RESTORE VERIFYONLY`). Keep the source
   database and backup until the new application has been accepted.
2. Create a separate PostgreSQL database/volume and apply `InitialPostgreSql`.
   Keep its API and seeder stopped until the import finishes. Do not remove old
   volumes with `docker compose down -v`.
3. Set the private environment variables below and run the transfer tool.
   The source account needs read access; the target account needs COPY, table
   locking and identity-sequence permissions. Run on Windows when using Windows
   integrated authentication against SQL Express.

```powershell
$env:SOURCE_SQLSERVER_CONNECTION = 'Server=SUNSHINE\SQLEXPRESS;Database=Product;Trusted_Connection=True;TrustServerCertificate=true'
$env:TARGET_POSTGRES_CONNECTION = 'Host=localhost;Port=15432;Database=Product;Username=vendor;Password=YOUR_LOCAL_PASSWORD'
dotnet run --project src/Product.DataMigration --configuration Release
Remove-Item Env:SOURCE_SQLSERVER_CONNECTION, Env:TARGET_POSTGRES_CONNECTION
```

The importer expects the historical schema through
`20260323105724_Invite status stirng value instead of enum int`. It also accepts
that schema with the two subsequent SQL Server migrations applied. It does not
modify SQL Server or import its EF migration history into PostgreSQL.

The importer rejects a populated target, locks all application tables, copies
parent rows before dependants, preserves IDs and password bytes, verifies counts
and SHA-256 digests of every row before commit, and restarts identity sequences
above imported IDs. All PostgreSQL changes are in one transaction. Failed copies
roll back, so an empty target can be retried after addressing the source issue.
SQL Server reads use a serializable transaction; maintenance mode is required to
avoid long blocking or inconsistent external writes during cutover.

Known historical role spellings and numeric values are normalized. Numeric roles
on vendor/operator child rows use the child-table type. Existing string `Admin`
roles are retained for business users. If the source lacks the legacy promotion
migration, administrators without a business become `SuperAdmin`, implementing
the pending app-wide administrator upgrade. Administrators assigned to a business
remain `Admin`. Source roles and hashes remain untouched.

Dates written as UTC by the application become PostgreSQL `timestamp with time
zone`; SQL Server's unspecified DateTime kind is interpreted as UTC. PostgreSQL
stores microseconds, so sub-microsecond ticks are truncated and ignored by digest
comparison. Password formats are copied unchanged: 64-byte legacy SHA-512 hashes
still upgrade after successful login, and current 57-byte hashes continue working.
Other pre-existing formats need separate recovery; migration cannot reconstruct
passwords. SQL Server collation behavior is not identical to PostgreSQL. Business
name searches explicitly remain case insensitive and treat `%` and `_` literally.

4. Start the PostgreSQL API using its new connection string. Use a fresh Redis
   instance/cache prefix so cached SQL Server entities cannot mask the imported
   data. A new JWT secret requires signing in again. Verify existing logins, roles,
   business lists, facilities/services and a new record with a generated ID.
5. Keep SQL Server available for rollback. If switching back after PostgreSQL has
   accepted new writes, those writes need reconciliation; they are not copied back
   automatically. Do not drop the old database as part of this step.

## Validation

```powershell
dotnet test src/Product.Tests --configuration Release
dotnet test src/Product.IntegrationTests --configuration Release
```

HTTP tests apply the real PostgreSQL migrations rather than `EnsureCreated`.
Dedicated tests cover production provider registration, repeated migrations,
synchronous/async UTC audit fields, case insensitive search and literal patterns.
Transfer tests generate the SQL Server schema from the historical migration
fixture, verify hashes/IDs/relationships/roles, reject repeated imports, and prove
that a late foreign-key failure rolls back earlier copies. Docker containers are
disposable and never use the application's local database.
