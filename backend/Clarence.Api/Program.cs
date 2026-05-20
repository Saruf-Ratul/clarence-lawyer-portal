using Clarence.Application;
using Clarence.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<ICaseService, CaseService>();
builder.Services.AddScoped<IRentManagerInboxService, RentManagerInboxService>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/api/dashboard/kpis", async (IDashboardService service, CancellationToken ct) => Results.Ok(await service.GetKpisAsync(ct)));
app.MapGet("/api/clients", async (IClientService service, CancellationToken ct) => Results.Ok(await service.GetClientsAsync(ct)));
app.MapGet("/api/clients/{clientId:guid}/properties", async (Guid clientId, IPropertyService service, CancellationToken ct) => Results.Ok(await service.GetPropertiesByClientAsync(clientId, ct)));
app.MapGet("/api/cases", async (ICaseService service, CancellationToken ct) => Results.Ok(await service.GetCasesAsync(ct)));
app.MapGet("/api/cases/{caseId:guid}", async (Guid caseId, ICaseService service, CancellationToken ct) =>
{
    var item = await service.GetCaseAsync(caseId, ct);
    return item is null ? Results.NotFound() : Results.Ok(item);
});
app.MapPost("/api/cases/{caseId:guid}/status", async (Guid caseId, UpdateCaseStatusRequest request, ICaseService service, CancellationToken ct) =>
{
    var item = await service.UpdateStatusAsync(caseId, request, ct);
    return item is null ? Results.BadRequest(new { message = "Invalid case or status" }) : Results.Ok(item);
});
app.MapGet("/api/rm-inbox", async (IRentManagerInboxService service, CancellationToken ct) => Results.Ok(await service.GetSubmissionsAsync(ct)));
app.MapPost("/api/rm-inbox/accept", async (AcceptSubmissionRequest request, IRentManagerInboxService service, CancellationToken ct) =>
{
    var result = await service.AcceptAsync(request, ct);
    return result is null ? Results.BadRequest(new { message = "Submission not found or already accepted" }) : Results.Ok(result);
});

app.Run();
