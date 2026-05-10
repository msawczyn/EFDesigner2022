using System;
using System.Diagnostics;
using System.IO;

using Microsoft.EntityFrameworkCore;
using FileBaseContext.Extensions;


namespace Ex2_ModelOne2One
{
    public partial class EFModelOne2One : DbContext
    {
       
        public static string DatabaseName = "EFModelOne2One"; // Will create folder \bin\my_local_db and tables.json files
        private static string SchemaVersion = "1.0"; // Update this version when schema changes
        private static string VersionFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DatabaseName, "schema_version.txt");


        partial void CustomInit(DbContextOptionsBuilder optionsBuilder)
        {

            if (HasSchemaChanged())
            {
                DeleteOldStore();
                SaveCurrentSchemaVersion();
            }

            if (Debugger.IsAttached)
            {
                optionsBuilder.EnableDetailedErrors();
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.EnableDetailedErrors();
            }
            optionsBuilder.UseFileBaseContextDatabase(databaseName: DatabaseName);
        }

        private bool HasSchemaChanged()
        {
            if (!File.Exists(VersionFilePath))
            {
                SaveCurrentSchemaVersion();
            }

            string storedVersion = File.ReadAllText(VersionFilePath);
            return !storedVersion.Equals(SchemaVersion, StringComparison.OrdinalIgnoreCase);
        }

        public void DeleteOldStore()
        {
            string contextPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DatabaseName);

            if (Directory.Exists(contextPath))
            {
                try
                {
                    Directory.Delete(contextPath, true);
                    Console.WriteLine("Old FileBasedContext store deleted successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while deleting the context store: {ex.Message}");
                }
            }
        }

        private void SaveCurrentSchemaVersion()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(VersionFilePath));
            File.WriteAllText(VersionFilePath, SchemaVersion);
        }
    }
}