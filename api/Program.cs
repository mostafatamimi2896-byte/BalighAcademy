using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var pg = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(pg))
{
    var uri = new Uri(pg);
    var parts = uri.UserInfo.Split(':', 2);
    var user = Uri.UnescapeDataString(parts[0]);
    var pass = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : "";
    var dbname = uri.AbsolutePath.TrimStart('/');
    var port = uri.Port > 0 ? uri.Port : 5432;

    var csb = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = port,
        Database = dbname,
        Username = user,
        Password = pass,
        Timeout = 10,
        SslMode = SslMode.Prefer,
        TrustServerCertificate = true
    };
    builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(csb.ToString()));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(o =>
        o.UseSqlServer(builder.Configuration.GetConnectionString("Default")
            ?? "Server=localhost;Database=BalighAcademy;Trusted_Connection=True;TrustServerCertificate=True;"));
}

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseCors();

app.MapGet("/", () => "✅ Baligh Academy API is running!");

app.MapGet("/api/dbinfo", () =>
{
    var p = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (string.IsNullOrEmpty(p)) return Results.Ok(new { set = false });
    var u = new Uri(p);
    return Results.Ok(new { set = true, host = u.Host, port = u.Port, db = u.AbsolutePath.TrimStart('/') });
});

app.MapGet("/api/health", async (AppDbContext db) =>
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
    try
    {
        var ok = await db.Database.CanConnectAsync(cts.Token);
        return Results.Ok(new { ok = true, db = ok });
    }
    catch (Exception ex)
    {
        return Results.Ok(new { ok = true, db = false, error = ex.Message });
    }
});

// فقط برای دوران ساخت و تست: بازسازی جدول‌ها با ساختار جدید
app.MapGet("/api/admin/reset", async (AppDbContext db) =>
{
    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();
    return Results.Ok(new { reset = true });
});

app.MapGet("/api/students", async (AppDbContext db) => await db.Students.ToListAsync());

app.MapPost("/api/students", async (AppDbContext db, Student s) =>
{
    db.Students.Add(s);
    await db.SaveChangesAsync();
    return Results.Created($"/api/students/{s.Id}", s);
});

_ = Task.Run(async () =>
{
    await Task.Delay(2000);
    try
    {
        using var scope = app.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        Console.WriteLine("DB init error: " + ex.Message);
    }
});

app.Run();

public class Student
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string FirstNameEn { get; set; } = "";
    public string LastNameEn { get; set; } = "";
    public string? Mobile { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Gender { get; set; }
    public string? BirthDate { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Student> Students => Set<Student>();
}
