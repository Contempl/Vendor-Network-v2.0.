using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Npgsql;
using NpgsqlTypes;

namespace Product.DataMigration;

public static class SqlServerToPostgres
{
    // Parent rows precede dependants; no constraints are disabled during the transfer.
    private static readonly string[] Tables =
    [
        "Businesses", "User", "Administrators", "Industries", "VendorsFacilities",
        "OperatorUsers", "VendorUsers", "Invites", "RefreshTokens", "VendorsFacilityServices"
    ];

    public static async Task<IReadOnlyDictionary<string, long>> TransferAsync(
        string sourceConnection, string targetConnection, CancellationToken cancellationToken = default)
    {
        await using var source = new SqlConnection(sourceConnection);
        await source.OpenAsync(cancellationToken);
        await using var sourceTransaction = (SqlTransaction)await source.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        await using var target = new NpgsqlConnection(targetConnection);
        await target.OpenAsync(cancellationToken);
        await using var targetTransaction = await target.BeginTransactionAsync(cancellationToken);

        await using var roleMigrationCheck = new SqlCommand("""
            SELECT COUNT(*) FROM [dbo].[__EFMigrationsHistory]
            WHERE [MigrationId] = '20261002120000_PromoteExistingAdministrators'
            """, source, sourceTransaction);
        var promoteLegacyAdministrators = (int)(await roleMigrationCheck.ExecuteScalarAsync(cancellationToken))! == 0;

        await using (var lockCommand = new NpgsqlCommand(
            $"LOCK TABLE {string.Join(", ", Tables.Select(Qualified))} IN ACCESS EXCLUSIVE MODE", target, targetTransaction))
            await lockCommand.ExecuteNonQueryAsync(cancellationToken);

        // Check every table before copying anything; refuse to merge or overwrite data.
        foreach (var table in Tables)
        {
            await using var emptyCheck = new NpgsqlCommand(
                $"SELECT EXISTS (SELECT 1 FROM {Qualified(table)})", target, targetTransaction);
            if ((bool)(await emptyCheck.ExecuteScalarAsync(cancellationToken))!)
                throw new InvalidOperationException("Target database is not empty.");
        }

        var counts = new Dictionary<string, long>();
        foreach (var table in Tables)
        {
            var columns = new List<(string Name, NpgsqlDbType Type)>();
            await using (var schema = new NpgsqlCommand("""
                SELECT column_name, udt_name
                FROM information_schema.columns
                WHERE table_schema = 'public' AND table_name = @table
                ORDER BY ordinal_position
                """, target, targetTransaction))
            {
                schema.Parameters.AddWithValue("table", table);
                await using var reader = await schema.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                    columns.Add((reader.GetString(0), MapType(reader.GetString(1))));
            }
            if (columns.Count == 0)
                throw new InvalidOperationException("Target schema is missing a table.");

            var sourceColumns = string.Join(", ", columns.Select(c => $"[{c.Name.Replace("]", "]]")}]"));
            await using var read = new SqlCommand($"SELECT {sourceColumns} FROM [dbo].[{table}] ORDER BY [Id]", source, sourceTransaction)
            {
                CommandTimeout = 300
            };
            await using var rows = await read.ExecuteReaderAsync(cancellationToken);
            var targetColumns = string.Join(", ", columns.Select(c => Quote(c.Name)));
            using var sourceDigest = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            await using (var copy = await target.BeginBinaryImportAsync(
                $"COPY {Qualified(table)} ({targetColumns}) FROM STDIN (FORMAT BINARY)", cancellationToken))
            {
                long count = 0;
                while (await rows.ReadAsync(cancellationToken))
                {
                    var rowValues = new object[columns.Count];
                    rows.GetValues(rowValues);
                    AppendRow(sourceDigest, rowValues);
                    await copy.StartRowAsync(cancellationToken);
                    for (var index = 0; index < columns.Count; index++)
                    {
                        if (await rows.IsDBNullAsync(index, cancellationToken))
                        {
                            await copy.WriteNullAsync(cancellationToken);
                            continue;
                        }
                        var value = rows.GetValue(index);
                        // SQL Server datetime2 has no Kind. The application has always written UTC.
                        if (value is DateTime timestamp)
                            value = DateTime.SpecifyKind(timestamp, DateTimeKind.Utc);
                        await copy.WriteAsync(value, columns[index].Type, cancellationToken);
                    }
                    count++;
                }
                await copy.CompleteAsync(cancellationToken);
                counts.Add(table, count);
            }

            await using var verify = new NpgsqlCommand($"SELECT COUNT(*) FROM {Qualified(table)}", target, targetTransaction);
            if ((long)(await verify.ExecuteScalarAsync(cancellationToken))! != counts[table])
                throw new InvalidOperationException("Transferred row count does not match.");

            using var targetDigest = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            await using var verifyRows = new NpgsqlCommand(
                $"SELECT {targetColumns} FROM {Qualified(table)} ORDER BY \"Id\"", target, targetTransaction);
            await using (var reader = await verifyRows.ExecuteReaderAsync(cancellationToken))
                while (await reader.ReadAsync(cancellationToken))
                {
                    var rowValues = new object[columns.Count];
                    reader.GetValues(rowValues);
                    AppendRow(targetDigest, rowValues);
                }
            if (!CryptographicOperations.FixedTimeEquals(sourceDigest.GetHashAndReset(), targetDigest.GetHashAndReset()))
                throw new InvalidOperationException($"Transferred row contents do not match in {table}.");
        }

        await using (var normalizeRoles = new NpgsqlCommand("""
            UPDATE "public"."User" u SET "UserType" = CASE
              WHEN lower(u."UserType") = 'admin' THEN 'Admin'
              WHEN lower(u."UserType") = 'superadmin' THEN 'SuperAdmin'
              WHEN lower(u."UserType") = 'vendoruser' THEN 'VendorUser'
              WHEN lower(u."UserType") = 'operatoruser' THEN 'OperatorUser'
              WHEN u."UserType" IN ('0', '1', '2', '3') THEN CASE
                WHEN EXISTS (SELECT 1 FROM "public"."VendorUsers" v WHERE v."Id" = u."Id") THEN 'VendorUser'
                WHEN EXISTS (SELECT 1 FROM "public"."OperatorUsers" o WHERE o."Id" = u."Id") THEN 'OperatorUser'
                WHEN u."UserType" = '1' THEN 'VendorUser'
                WHEN u."UserType" = '2' THEN 'OperatorUser'
                WHEN u."UserType" = '3' THEN 'SuperAdmin'
                ELSE 'Admin' END
              ELSE u."UserType" END
            """, target, targetTransaction))
            await normalizeRoles.ExecuteNonQueryAsync(cancellationToken);

        if (promoteLegacyAdministrators)
        {
            await using var promote = new NpgsqlCommand("""
                UPDATE "public"."User" SET "UserType" = 'SuperAdmin'
                WHERE "UserType" = 'Admin' AND "BusinessId" IS NULL
                  AND "Id" IN (SELECT "Id" FROM "public"."Administrators")
                """, target, targetTransaction);
            await promote.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (var table in Tables)
        {
            await using var sequenceQuery = new NpgsqlCommand(
                "SELECT pg_get_serial_sequence(@table, 'Id')", target, targetTransaction);
            sequenceQuery.Parameters.AddWithValue("table", Qualified(table));
            var sequence = await sequenceQuery.ExecuteScalarAsync(cancellationToken) as string;
            if (sequence is null) continue; // TPT child tables share their parent's IDs.
            await using var maximumQuery = new NpgsqlCommand(
                $"SELECT COALESCE(MAX(\"Id\"), 0)::bigint FROM {Qualified(table)}", target, targetTransaction);
            var maximum = (long)(await maximumQuery.ExecuteScalarAsync(cancellationToken))!;
            // ALTER SEQUENCE is transactional, unlike setval().
            await using var reset = new NpgsqlCommand(
                $"ALTER SEQUENCE {sequence} RESTART WITH {checked(maximum + 1)}", target, targetTransaction);
            await reset.ExecuteNonQueryAsync(cancellationToken);
        }

        await sourceTransaction.CommitAsync(cancellationToken);
        await targetTransaction.CommitAsync(cancellationToken);
        return counts;
    }

    private static string Quote(string name) => $"\"{name.Replace("\"", "\"\"")}\"";
    private static string Qualified(string table) => $"\"public\".{Quote(table)}";

    private static void AppendRow(IncrementalHash digest, object[] values)
    {
        for (var index = 0; index < values.Length; index++)
            values[index] = values[index] switch
            {
                DBNull => null!,
                DateTime timestamp => new DateTime(timestamp.Ticks - timestamp.Ticks % 10, DateTimeKind.Utc),
                float number => (double)number, // SQL Server float(<=24) is single precision; PostgreSQL widens it.
                _ => values[index]
            };
        digest.AppendData(JsonSerializer.SerializeToUtf8Bytes(values));
        digest.AppendData("\n"u8);
    }

    private static NpgsqlDbType MapType(string type) => type switch
    {
        "int4" => NpgsqlDbType.Integer,
        "int8" => NpgsqlDbType.Bigint,
        "float8" => NpgsqlDbType.Double,
        "bool" => NpgsqlDbType.Boolean,
        "bytea" => NpgsqlDbType.Bytea,
        "varchar" => NpgsqlDbType.Varchar,
        "text" => NpgsqlDbType.Text,
        "timestamptz" => NpgsqlDbType.TimestampTz,
        _ => throw new InvalidOperationException("Unsupported target column type.")
    };
}
