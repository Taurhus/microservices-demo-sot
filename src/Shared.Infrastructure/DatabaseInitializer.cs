using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Shared.Infrastructure;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync<TContext>(
        IServiceProvider serviceProvider,
        string databaseName,
        ILogger? logger = null) where TContext : DbContext
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();
        
        const int maxRetries = 15;
        const int delayMs = 3000;
        var retries = 0;
        var dbReady = false;

        logger?.LogInformation("Waiting for database {DatabaseName} to be ready...", databaseName);

        // First, try to create the database if it doesn't exist
        var connectionString = db.Database.GetConnectionString();
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string is null or empty");
        }

        // Build master connection string for database creation
        var builder = new SqlConnectionStringBuilder(connectionString);
        builder.InitialCatalog = "master";
        var masterConnectionString = builder.ConnectionString;

        // Try to create database
        try
        {
            using var masterConn = new SqlConnection(masterConnectionString);
            await masterConn.OpenAsync();
            using var cmd = masterConn.CreateCommand();
            cmd.CommandText = $@"
                IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{databaseName}')
                BEGIN
                    CREATE DATABASE [{databaseName}];
                END";
            await cmd.ExecuteNonQueryAsync();
            logger?.LogInformation("Database {DatabaseName} created or already exists", databaseName);
        }
        catch (Exception ex)
        {
            logger?.LogWarning("Could not create database {DatabaseName}: {Error}", databaseName, ex.Message);
        }

        // Now wait for database to be accessible
        while (retries < maxRetries && !dbReady)
        {
            try
            {
                dbReady = await db.Database.CanConnectAsync();
            }
            catch (Exception ex)
            {
                logger?.LogWarning("Database connection attempt {Attempt}/{MaxRetries} failed: {Error}", 
                    retries + 1, maxRetries, ex.Message);
                dbReady = false;
            }

            if (!dbReady)
            {
                retries++;
                logger?.LogInformation("Waiting for database... attempt {Attempt}/{MaxRetries}", retries, maxRetries);
                await Task.Delay(delayMs);
            }
        }

        if (!dbReady)
        {
            logger?.LogError("Could not connect to database {DatabaseName} after {MaxRetries} attempts", 
                databaseName, maxRetries);
            throw new InvalidOperationException(
                $"Could not connect to database '{databaseName}' after {maxRetries} attempts.");
        }

        logger?.LogInformation("Database {DatabaseName} is ready. Checking for migrations...", databaseName);
        
        // Check if migrations exist by trying to get them
        // If no migrations exist, use EnsureCreated instead of Migrate
        var hasMigrations = false;
        try
        {
            var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
            var appliedMigrations = await db.Database.GetAppliedMigrationsAsync();
            var allMigrations = pendingMigrations.Concat(appliedMigrations);
            hasMigrations = allMigrations.Any();
        }
        catch
        {
            // If we can't check migrations, assume there are none
            hasMigrations = false;
        }
        
        if (hasMigrations)
        {
            logger?.LogInformation("Migrations found. Applying migrations...");
            try
            {
                await db.Database.MigrateAsync();
                logger?.LogInformation("Migrations applied successfully for {DatabaseName}", databaseName);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to apply migrations. Falling back to EnsureCreated...");
                await db.Database.EnsureCreatedAsync();
                logger?.LogInformation("Database schema ensured for {DatabaseName}", databaseName);
            }
        }
        else
        {
            logger?.LogInformation("No migrations found. Ensuring database schema exists...");
            
            // If __EFMigrationsHistory table exists but no migrations were applied,
            // drop it first so EnsureCreated can work properly
            try
            {
                var connection = db.Database.GetDbConnection();
                var wasOpen = connection.State == System.Data.ConnectionState.Open;
                if (!wasOpen)
                {
                    await connection.OpenAsync();
                }
                
                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__EFMigrationsHistory')
                    BEGIN
                        DROP TABLE [__EFMigrationsHistory];
                    END";
                await cmd.ExecuteNonQueryAsync();
                
                if (!wasOpen)
                {
                    await connection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Could not clean up migrations history table");
            }
            
            await db.Database.EnsureCreatedAsync();
            logger?.LogInformation("Database schema ensured for {DatabaseName}", databaseName);
        }
    }
}

