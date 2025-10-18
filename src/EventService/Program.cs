using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using EventService.Models;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EventDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=azuresql;Database=EventDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Apply migrations at startup with wait/retry for DB connectivity and create DB if missing
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EventDb>();
    var maxRetries = 10;
    var delayMs = 3000;
    var retries = 0;
    bool dbReady = false;
    string dbName = "EventDb";
    string masterConnStr = "Server=azuresql;Database=master;User Id=sa;Password=Your_password123;TrustServerCertificate=True;";
    while (retries < maxRetries && !dbReady)
    {
        try
        {
            dbReady = db.Database.CanConnect();
        }
        catch
        {
            dbReady = false;
        }
        if (!dbReady)
        {
            retries++;
            Console.WriteLine($"Waiting for database... attempt {retries}/{maxRetries}");
            Thread.Sleep(delayMs);
        }
    }
    if (!dbReady)
    {
        // Try to create the database from master if it does not exist
        try
        {
            Console.WriteLine($"Database '{dbName}' does not exist or is not accessible. Attempting to create from master...");
            using (var masterConn = new SqlConnection(masterConnStr))
            {
                masterConn.Open();
                using (var cmd = masterConn.CreateCommand())
                {
                    cmd.CommandText = $"CREATE DATABASE [{dbName}];";
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex) when (ex.Number == 1801)
                    {
                        // Database already exists, safe to ignore
                    }
                }
            }
            // Now try to connect again, with retry to ensure DB is fully online
            var postCreateRetries = 10;
            var postCreateDelayMs = 2000;
            dbReady = false;
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
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database creation from master failed: {ex.Message}");
            throw new Exception("Could not connect or create the database after multiple attempts.");
        }
    }
    // Ensure schema is up to date
    db.Database.Migrate();
}

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
app.MapControllers();
app.Run();




