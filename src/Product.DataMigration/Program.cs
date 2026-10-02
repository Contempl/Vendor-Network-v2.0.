using Product.DataMigration;

var source = Environment.GetEnvironmentVariable("SOURCE_SQLSERVER_CONNECTION");
var target = Environment.GetEnvironmentVariable("TARGET_POSTGRES_CONNECTION");
if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target))
{
    Console.Error.WriteLine("Set SOURCE_SQLSERVER_CONNECTION and TARGET_POSTGRES_CONNECTION. Stop the source app and back up both databases first.");
    return 1;
}

try
{
    var counts = await SqlServerToPostgres.TransferAsync(source, target);
    foreach (var (table, count) in counts)
        Console.WriteLine($"{table}: {count} rows");
    Console.WriteLine("Transfer committed. Clear application Redis cache before starting the PostgreSQL app.");
    return 0;
}
catch (Exception exception)
{
    // Driver errors may include row contents. Do not print credentials or personal data.
    Console.Error.WriteLine($"Transfer failed ({exception.GetType().Name}); source data was not modified. Check target state before retrying; consult the migration guide.");
    return 1;
}
