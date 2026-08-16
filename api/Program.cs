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

app.MapGet("/api/admin/reset", async (AppDbContext db) =>
{
    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();
    return Results.Ok(new { reset = true });
});

app.MapGet("/api/students", async (AppDbContext db, string? q) =>
{
    var list = db.Students.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
    {
        q = q.Trim().ToLower();
        list = list.Where(s => s.FirstName.ToLower().Contains(q) || s.LastName.ToLower().Contains(q)
            || s.StudentCode.ToLower().Contains(q)
            || s.FirstNameEn.ToLower().Contains(q) || s.LastNameEn.ToLower().Contains(q)
            || (s.Mobile != null && s.Mobile.ToLower().Contains(q)));
    }
    return await list.OrderByDescending(s => s.Id).ToListAsync();
});

app.MapGet("/api/students/{id:int}", async (AppDbContext db, int id) =>
    await db.Students.FindAsync(id) is Student s ? Results.Ok(s) : Results.NotFound());

app.MapPost("/api/students", async (AppDbContext db, Student s) =>
{
    db.Students.Add(s);
    await db.SaveChangesAsync();
    if (string.IsNullOrEmpty(s.StudentCode))
    {
        s.StudentCode = $"BA-{1000 + s.Id}";
        await db.SaveChangesAsync();
    }
    return Results.Created($"/api/students/{s.Id}", s);
});

app.MapPut("/api/students/{id:int}", async (AppDbContext db, int id, Student input) =>
{
    var s = await db.Students.FindAsync(id);
    if (s is null) return Results.NotFound();
    s.FirstName = input.FirstName; s.LastName = input.LastName;
    s.FirstNameEn = input.FirstNameEn; s.LastNameEn = input.LastNameEn;
    s.Gender = input.Gender; s.BirthDate = input.BirthDate;
    s.Mobile = input.Mobile; s.Phone = input.Phone; s.Email = input.Email;
    s.Address = input.Address; s.Notes = input.Notes;
    s.PhotoBase64 = input.PhotoBase64; s.QuotaType = input.QuotaType; s.Status = input.Status;
    await db.SaveChangesAsync();
    return Results.Ok(s);
});

app.MapDelete("/api/students/{id:int}", async (AppDbContext db, int id) =>
{
    var s = await db.Students.FindAsync(id);
    if (s is null) return Results.NotFound();
    db.Students.Remove(s);
    await db.SaveChangesAsync();
    return Results.Ok(new { deleted = true });
});
app.MapGet("/api/terms", async (AppDbContext db) => await db.Terms.OrderByDescending(t => t.Id).ToListAsync());
app.MapPost("/api/terms", async (AppDbContext db, Term t) => { db.Terms.Add(t); await db.SaveChangesAsync(); return Results.Ok(t); });

app.MapGet("/api/students/{id:int}/enrollments", async (AppDbContext db, int id) =>
    await db.Enrollments.Where(e => e.StudentId == id).ToListAsync());
app.MapPost("/api/students/{id:int}/enrollments", async (AppDbContext db, int id, Enrollment e) =>
{
    e.StudentId = id;
    db.Enrollments.Add(e);
    await db.SaveChangesAsync();
    return Results.Ok(e);
});
app.MapDelete("/api/enrollments/{id:int}", async (AppDbContext db, int id) =>
{
    var e = await db.Enrollments.FindAsync(id);
    if (e is null) return Results.NotFound();
    db.Enrollments.Remove(e);
    await db.SaveChangesAsync();
    return Results.Ok(new { deleted = true });
});

app.MapGet("/api/students/{id:int}/payments", async (AppDbContext db, int id) =>
    await db.Payments.Where(p => p.StudentId == id).ToListAsync());
app.MapPost("/api/students/{id:int}/payments", async (AppDbContext db, int id, Payment p) =>
{
    p.StudentId = id;
    db.Payments.Add(p);
    await db.SaveChangesAsync();
    return Results.Ok(p);
});
app.MapDelete("/api/payments/{id:int}", async (AppDbContext db, int id) =>
{
    var p = await db.Payments.FindAsync(id);
    if (p is null) return Results.NotFound();
    db.Payments.Remove(p);
    await db.SaveChangesAsync();
    return Results.Ok(new { deleted = true });
});

app.MapGet("/api/students/{id:int}/finance", async (AppDbContext db, int id) =>
{
    var pays = await db.Payments.Where(p => p.StudentId == id).ToListAsync();
    long tuition = pays.Where(p => p.Kind.StartsWith("شهریه")).Sum(p => p.Amount);
    long paid = pays.Where(p => p.Kind.StartsWith("پرداخت")).Sum(p => p.Amount);
    long discount = pays.Where(p => p.Kind.StartsWith("تخفیف")).Sum(p => p.Amount);
    long balance = tuition - paid - discount;
    return Results.Ok(new { tuition, paid, discount, balance, debtor = balance > 0 });
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
    public string StudentCode { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string FirstNameEn { get; set; } = "";
    public string LastNameEn { get; set; } = "";
    public string? Gender { get; set; }
    public string? BirthDate { get; set; }
    public string? Mobile { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public string? PhotoBase64 { get; set; }
    public string QuotaType { get; set; } = "عادی / Regular";
    public string Status { get; set; } = "فعال / Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Term> Terms => Set<Term>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Payment> Payments => Set<Payment>();
}

public class Term
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string StartDate { get; set; } = "";
    public string EndDate { get; set; } = "";
    public bool IsCurrent { get; set; }
}

public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int TermId { get; set; }
    public string ClassName { get; set; } = "";
    public string Level { get; set; } = "";
    public string Result { get; set; } = "در حال برگزاری / Ongoing";
    public double Score { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Payment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string Date { get; set; } = "";
    public long Amount { get; set; }
    public string Kind { get; set; } = "شهریه / Tuition";
    public string Note { get; set; } = "";
}
