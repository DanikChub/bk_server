using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using KV.Server.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var root = Path.GetFullPath(args.Length > 0 ? args[0] : "../../..");
var output = Path.Combine(root, "database", ".local", "export");
Directory.CreateDirectory(output);
using var context = new ServerDbContext();
var utf8 = new UTF8Encoding(false);
File.WriteAllText(Path.Combine(output, "snapshot.sql"), context.Database.GenerateCreateScript(), utf8);

var assembly = context.GetService<IMigrationsAssembly>();
var migrations = assembly.Migrations.OrderBy(entry => entry.Key).ToArray();
var rawSql = new List<object>();
foreach (var entry in migrations)
{
    var migration = assembly.CreateMigration(entry.Value, context.Database.ProviderName);
    foreach (var operation in migration.UpOperations.OfType<SqlOperation>())
        rawSql.Add(new { migration = entry.Key, sql = operation.Sql, suppressTransaction = operation.SuppressTransaction });
}
File.WriteAllText(Path.Combine(output, "raw-operations.json"), JsonSerializer.Serialize(rawSql, new JsonSerializerOptions { WriteIndented = true }), utf8);
File.WriteAllText(Path.Combine(output, "migration-ids.json"), JsonSerializer.Serialize(migrations.Select(entry => entry.Key), new JsonSerializerOptions { WriteIndented = true }), utf8);
File.WriteAllText(Path.Combine(output, "replay.sql"), context.GetService<IMigrator>().GenerateScript(), utf8);

var sourceDirectory = Path.Combine(root, "KV.Server", "src", "KV.Server.EntityFrameworkCore", "Migrations");
var hashes = Directory.GetFiles(sourceDirectory, "*.cs").OrderBy(Path.GetFileName)
    .Select(path => new { file = Path.GetFileName(path), sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant() });
File.WriteAllText(Path.Combine(output, "source-hashes.json"), JsonSerializer.Serialize(hashes, new JsonSerializerOptions { WriteIndented = true }), utf8);
Console.WriteLine($"Exported snapshot and {migrations.Length} migrations to {output}");
