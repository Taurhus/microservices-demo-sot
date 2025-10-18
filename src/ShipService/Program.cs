
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using ShipService;

// Wait for SQL TCP port to be reachable before any SQL logic
string sqlHost = "azuresql";
int sqlPort = 1433;
int maxPortWait = 30;
int portWaitDelay = 2000;
bool portOpen = false;
for (int i = 0; i < maxPortWait && !portOpen; i++)
{
    try
    {
        using var client = new TcpClient();
        var task = client.ConnectAsync(sqlHost, sqlPort);
        portOpen = task.Wait(2000) && client.Connected;
    }
    catch { portOpen = false; }
    if (!portOpen) Thread.Sleep(portWaitDelay);
}
if (!portOpen)
    throw new Exception($"[ShipService] Could not connect to SQL TCP port {sqlHost}:{sqlPort} after {maxPortWait} attempts");

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

// Ensure ShipDb exists before configuring EF, with retry
string dbName = "ShipDb";
string masterConnStr = "Server=azuresql;Database=master;User Id=sa;Password=Your_password123;TrustServerCertificate=True;";
int maxDbCreateRetries = 15;
int dbCreateDelayMs = 4000;
bool dbCreated = false;
for (int i = 0; i < maxDbCreateRetries && !dbCreated; i++)
{
    try
    {
        using var masterConn = new SqlConnection(masterConnStr);
        masterConn.Open();
        using var cmd = masterConn.CreateCommand();
        cmd.CommandText = $"IF DB_ID('{dbName}') IS NULL CREATE DATABASE [{dbName}]";
        cmd.ExecuteNonQuery();
        dbCreated = true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ShipService] Waiting for SQL to be ready for DB creation: {ex.Message}");
        Thread.Sleep(dbCreateDelayMs);
    }
}
if (!dbCreated)
{
    throw new Exception($"[ShipService] Could not create database {dbName} after {maxDbCreateRetries} attempts");
}

builder.Services.AddDbContext<ShipDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=azuresql;Database=ShipDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var app = builder.Build();

// Apply migrations at startup with wait/retry for DB connectivity
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShipDb>();
    // After DB creation, retry connecting to ensure DB is online before migration
    var postCreateRetries = 10;
    var postCreateDelayMs = 2000;
    bool dbReady = false;
    for (int i = 0; i < postCreateRetries; i++)
    {
        try
        {
            dbReady = db.Database.CanConnect();
            if (dbReady) break;
        }
        catch { dbReady = false; }
        Thread.Sleep(postCreateDelayMs);
    }
    if (!dbReady)
    {
        throw new Exception($"[ShipService] Could not connect to database {dbName} after {postCreateRetries} attempts");
    }
    db.Database.Migrate();
}

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Redirect root to Swagger UI
    app.Use(async (context, next) => {
        if (context.Request.Path == "/")
        {
            context.Response.Redirect("/swagger");
            return;
        }
        await next();
    });
}

app.UseHttpsRedirection();

// All minimal API endpoints removed; only controller setup remains.

app.Run();


