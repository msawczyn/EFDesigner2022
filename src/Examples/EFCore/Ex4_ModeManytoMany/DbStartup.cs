using System;
using System.Diagnostics;

using Microsoft.EntityFrameworkCore;
using FileBaseContext.Extensions;
using System.IO;

namespace Ex4_ModelManytoMany
{
    public partial class EFModelManytoMany : DbContext
    {
        // In EF Core you add in a Database provider
        // FileBaseContext is a provider of Entity Framework Core 8 to store database information in files.
        // https://github.com/Opzet/FileBaseContext
        // NUGET EntityFilesystem
        // using FileBaseContext.Extensions;
        /// </summary>
        public static string DatabaseName = "EFModelManytoMany"; // Will create folder \bin\my_local_db and tables.json files
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