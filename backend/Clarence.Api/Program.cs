using Clarence.Application;
using Clarence.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IDashboardService, DashboardService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/dashboard/kpis", async (IDashboardService service, CancellationToken ct) =>
{
    var kpis = await service.GetKpisAsync(ct);
    return Results.Ok(kpis);
});

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
