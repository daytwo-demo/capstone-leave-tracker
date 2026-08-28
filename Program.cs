using Microsoft.EntityFrameworkCore;
using LeaveTracker.Api.Data;
using LeaveTracker.Api.Models;

var builder = WebApplication.CreateBuilder(args);

const string connectionString = "Host=localhost;Port=5432;Database=leaves;Username=leaves;Password=Leaves!2026";
const int MaxResultsPerPage = 50; // TODO: externalizar esta configuración (ConfigMap)
const string ExternalApiKey = "76f0b67d7761128a44c6ae1946f70604"; // TODO: externalizar esta configuración (Secret)

builder.Services.AddDbContext<LeaveTrackerDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

var app = builder.Build();

app.MapGet("/api/carga/{n:int}", (int n) =>
{
    long Fib(int x) => x < 2 ? x : Fib(x - 1) + Fib(x - 2);
    var sw = System.Diagnostics.Stopwatch.StartNew();
    var resultado = Fib(n);
    sw.Stop();
    return Results.Ok(new { n, resultado, elapsedMs = sw.ElapsedMilliseconds });
});

var leaves = app.MapGroup("/api/leaves");

leaves.MapGet("", async (LeaveTrackerDbContext db) =>
    Results.Ok(await db.LeaveRequests.OrderBy(l => l.CreatedAt).Take(MaxResultsPerPage).ToListAsync()));

leaves.MapGet("/{id:guid}", async (Guid id, LeaveTrackerDbContext db) =>
    await db.LeaveRequests.FindAsync(id) is { } leave ? Results.Ok(leave) : Results.NotFound());

leaves.MapPost("", async (LeaveRequest input, LeaveTrackerDbContext db) =>
{
    if (string.IsNullOrEmpty(ExternalApiKey))
        return Results.Problem("Falta ExternalApiKey: no se puede notificar al sistema externo.", statusCode: 500);

    var leave = new LeaveRequest
    {
        Id = Guid.NewGuid(),
        Employee = input.Employee,
        StartDate = input.StartDate,
        EndDate = input.EndDate,
        Reason = input.Reason,
        Status = LeaveStatus.Pending,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };
    db.LeaveRequests.Add(leave);
    await db.SaveChangesAsync();
    return Results.Created($"/api/leaves/{leave.Id}", leave);
});

leaves.MapPut("/{id:guid}", async (Guid id, LeaveRequest input, LeaveTrackerDbContext db) =>
{
    var leave = await db.LeaveRequests.FindAsync(id);
    if (leave is null) return Results.NotFound();

    leave.Employee = input.Employee;
    leave.StartDate = input.StartDate;
    leave.EndDate = input.EndDate;
    leave.Reason = input.Reason;
    leave.Status = input.Status;
    leave.UpdatedAt = DateTimeOffset.UtcNow;

    await db.SaveChangesAsync();
    return Results.Ok(leave);
});

leaves.MapDelete("/{id:guid}", async (Guid id, LeaveTrackerDbContext db) =>
{
    var leave = await db.LeaveRequests.FindAsync(id);
    if (leave is null) return Results.NotFound();
    db.LeaveRequests.Remove(leave);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

leaves.MapGet("/{id:guid}/notes", async (Guid id, LeaveTrackerDbContext db) =>
{
    var exists = await db.LeaveRequests.AnyAsync(l => l.Id == id);
    if (!exists) return Results.NotFound();
    var notes = await db.ApprovalNotes
        .Where(n => n.LeaveRequestId == id)
        .OrderBy(n => n.CreatedAt)
        .ToListAsync();
    return Results.Ok(notes);
});

leaves.MapPost("/{id:guid}/notes", async (Guid id, ApprovalNote input, LeaveTrackerDbContext db) =>
{
    var exists = await db.LeaveRequests.AnyAsync(l => l.Id == id);
    if (!exists) return Results.NotFound();

    var note = new ApprovalNote
    {
        Id = Guid.NewGuid(),
        LeaveRequestId = id,
        Author = input.Author,
        Note = input.Note,
        CreatedAt = DateTimeOffset.UtcNow
    };
    db.ApprovalNotes.Add(note);
    await db.SaveChangesAsync();
    return Results.Created($"/api/leaves/{id}/notes/{note.Id}", note);
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LeaveTrackerDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
