using Clarence.Application;
using Clarence.Domain;

namespace Clarence.Infrastructure;

public static class FakeStore
{
    public static readonly List<Client> Clients =
    [
        new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "ABC Properties", RentManagerAccountId = "RM-ABC-001" },
        new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111112"), Name = "Garden State Homes", RentManagerAccountId = "RM-GSH-002" }
    ];
    public static readonly List<Property> Properties =
    [
        new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222221"), ClientId = Clients[0].Id, AddressLine1 = "123 Main St", City = "Newark", PostalCode = "07102" },
        new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), ClientId = Clients[0].Id, AddressLine1 = "456 Oak Ave", City = "Jersey City", PostalCode = "07302" }
    ];
    public static readonly List<Tenant> Tenants =
    [
        new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333331"), FullName = "John Doe", Unit = "2B" },
        new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333332"), FullName = "Maria Santos", Unit = "4A" }
    ];
    public static readonly List<LegalCase> Cases =
    [
        new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444441"), CaseNumber = "CL-2026-1184", ClientId = Clients[0].Id, PropertyId = Properties[0].Id, TenantId = Tenants[0].Id, MonthlyRent = 1800, BalanceOwed = 5400, LateFees = 150, CourtCosts = 75, Stage = CaseStage.Accepted, NextDeadline = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(4)) }
    ];
    public static readonly List<RentManagerSubmission> Submissions = [];
    public static readonly List<CaseTimelineEvent> Timeline =
    [
        new() { Id = Guid.NewGuid(), LegalCaseId = Cases[0].Id, AtUtc = DateTime.UtcNow.AddDays(-3), Stage = CaseStage.Accepted, Actor = "Atty Rivera", Notes = "Case accepted from RM intake" }
    ];
}

public class DashboardService : IDashboardService
{
    public Task<DashboardKpiDto> GetKpisAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(new DashboardKpiDto(FakeStore.Clients.Count, FakeStore.Properties.Count, FakeStore.Cases.Count, FakeStore.Submissions.Count(x => !x.Accepted)));
}
public class ClientService : IClientService
{
    public Task<IReadOnlyList<ClientSummaryDto>> GetClientsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult((IReadOnlyList<ClientSummaryDto>)FakeStore.Clients.Select(c => new ClientSummaryDto(c.Id, c.Name, FakeStore.Properties.Count(p => p.ClientId == c.Id), FakeStore.Cases.Count(k => k.ClientId == c.Id), "RM Synced")).ToList());
}
public class PropertyService : IPropertyService
{
    public Task<IReadOnlyList<PropertySummaryDto>> GetPropertiesByClientAsync(Guid clientId, CancellationToken cancellationToken = default)
        => Task.FromResult((IReadOnlyList<PropertySummaryDto>)FakeStore.Properties.Where(p => p.ClientId == clientId).Select(p => new PropertySummaryDto(p.Id, p.ClientId, $"{p.AddressLine1}, {p.City}", FakeStore.Cases.Count(k => k.PropertyId == p.Id), "RM Synced")).ToList());
}
public class CaseService : ICaseService
{
    public Task<IReadOnlyList<CaseSummaryDto>> GetCasesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult((IReadOnlyList<CaseSummaryDto>)FakeStore.Cases.Select(MapSummary).ToList());
    public Task<CaseDetailDto?> GetCaseAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var lc = FakeStore.Cases.FirstOrDefault(x => x.Id == caseId);
        return Task.FromResult(lc is null ? null : MapDetail(lc));
    }
    public Task<CaseDetailDto?> UpdateStatusAsync(Guid caseId, UpdateCaseStatusRequest request, CancellationToken cancellationToken = default)
    {
        var lc = FakeStore.Cases.FirstOrDefault(x => x.Id == caseId);
        if (lc is null || !Enum.TryParse<CaseStage>(request.Stage, true, out var stage)) return Task.FromResult<CaseDetailDto?>(null);
        lc.Stage = stage;
        FakeStore.Timeline.Add(new CaseTimelineEvent { Id = Guid.NewGuid(), LegalCaseId = lc.Id, AtUtc = DateTime.UtcNow, Stage = stage, Actor = "Portal User", Notes = request.Notes });
        return Task.FromResult<CaseDetailDto?>(MapDetail(lc));
    }
    private static CaseSummaryDto MapSummary(LegalCase lc)
    {
        var client = FakeStore.Clients.First(x => x.Id == lc.ClientId);
        var prop = FakeStore.Properties.First(x => x.Id == lc.PropertyId);
        var tenant = FakeStore.Tenants.First(x => x.Id == lc.TenantId);
        return new CaseSummaryDto(lc.Id, lc.CaseNumber, client.Name, $"{prop.AddressLine1}, {prop.City}", tenant.FullName, lc.Stage.ToString(), lc.BalanceOwed, lc.NextDeadline);
    }
    private static CaseDetailDto MapDetail(LegalCase lc)
    {
        var s = MapSummary(lc);
        var tenant = FakeStore.Tenants.First(x => x.Id == lc.TenantId);
        var timeline = FakeStore.Timeline.Where(t => t.LegalCaseId == lc.Id).OrderBy(t => t.AtUtc).Select(t => new TimelineEventDto(t.AtUtc, t.Stage.ToString(), t.Actor, t.Notes)).ToList();
        return new CaseDetailDto(lc.Id, lc.CaseNumber, s.ClientName, s.PropertyAddress, s.TenantName, tenant.Unit, lc.MonthlyRent, lc.BalanceOwed, lc.LateFees, lc.CourtCosts, lc.NextDeadline, lc.Stage.ToString(), timeline);
    }
}
public class RentManagerInboxService : IRentManagerInboxService
{
    public Task<IReadOnlyList<RmSubmissionDto>> GetSubmissionsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult((IReadOnlyList<RmSubmissionDto>)FakeStore.Submissions.Where(s => !s.Accepted).Select(MapSubmission).ToList());

    public Task<CaseDetailDto?> AcceptAsync(AcceptSubmissionRequest request, CancellationToken cancellationToken = default)
    {
        var sub = FakeStore.Submissions.FirstOrDefault(x => x.Id == request.SubmissionId && !x.Accepted);
        if (sub is null) return Task.FromResult<CaseDetailDto?>(null);
        sub.Accepted = true;
        var legalCase = new LegalCase { Id = Guid.NewGuid(), CaseNumber = $"CL-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(2000, 9999)}", ClientId = sub.ClientId, PropertyId = sub.PropertyId, TenantId = sub.TenantId, MonthlyRent = sub.MonthlyRent, BalanceOwed = sub.BalanceOwed, LateFees = 0, CourtCosts = 75, Stage = CaseStage.Accepted, NextDeadline = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)) };
        FakeStore.Cases.Add(legalCase);
        FakeStore.Timeline.Add(new CaseTimelineEvent { Id = Guid.NewGuid(), LegalCaseId = legalCase.Id, AtUtc = DateTime.UtcNow, Stage = CaseStage.Accepted, Actor = request.AcceptedBy, Notes = "Accepted from RM Intake Inbox" });
        return new CaseService().GetCaseAsync(legalCase.Id, cancellationToken);
    }

    internal static RmSubmissionDto MapSubmission(RentManagerSubmission s)
    {
        var c = FakeStore.Clients.First(x => x.Id == s.ClientId);
        var p = FakeStore.Properties.First(x => x.Id == s.PropertyId);
        var t = FakeStore.Tenants.First(x => x.Id == s.TenantId);
        return new RmSubmissionDto(s.Id, c.Name, $"{p.AddressLine1}, {p.City}", t.FullName, t.Unit, s.BalanceOwed, s.MonthlyRent, s.ReceivedAtUtc, s.VerificationStatus);
    }
}

public class RentManagerSyncService : IRentManagerSyncService
{
    public Task<RmImportResult> PullLatestAsync(RmImportRequest request, CancellationToken cancellationToken = default)
    {
        // Placeholder for real RM API pull. Requires customer API credentials.
        var newClient = new Client { Id = Guid.NewGuid(), Name = "RM Imported Client", RentManagerAccountId = "RM-IMPORTED-001" };
        var newProperty = new Property { Id = Guid.NewGuid(), ClientId = newClient.Id, AddressLine1 = "789 Pine Rd", City = "Elizabeth", PostalCode = "07201" };
        var newTenant = new Tenant { Id = Guid.NewGuid(), FullName = "Imported Tenant", Unit = "3C" };
        var newSubmission = new RentManagerSubmission { Id = Guid.NewGuid(), ClientId = newClient.Id, PropertyId = newProperty.Id, TenantId = newTenant.Id, MonthlyRent = 1950, BalanceOwed = 3900, ReceivedAtUtc = DateTime.UtcNow, VerificationStatus = "1:1 RM Match" };

        FakeStore.Clients.Add(newClient);
        FakeStore.Properties.Add(newProperty);
        FakeStore.Tenants.Add(newTenant);
        FakeStore.Submissions.Add(newSubmission);

        return Task.FromResult(new RmImportResult(1, 1, 1, 1));
    }
}

public class FormService : IFormService
{
    public Task<NjFormDto?> BuildNjLtFormAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var legalCase = FakeStore.Cases.FirstOrDefault(c => c.Id == caseId);
        if (legalCase is null) return Task.FromResult<NjFormDto?>(null);
        var client = FakeStore.Clients.First(c => c.Id == legalCase.ClientId);
        var property = FakeStore.Properties.First(p => p.Id == legalCase.PropertyId);
        var tenant = FakeStore.Tenants.First(t => t.Id == legalCase.TenantId);

        var form = new NjFormDto(
            legalCase.Id,
            legalCase.CaseNumber,
            client.Name,
            tenant.FullName,
            property.AddressLine1,
            tenant.Unit,
            legalCase.MonthlyRent,
            legalCase.BalanceOwed,
            legalCase.LateFees,
            legalCase.CourtCosts,
            legalCase.NextDeadline,
            "Essex",
            DateTime.UtcNow);

        return Task.FromResult<NjFormDto?>(form);
    }
}
