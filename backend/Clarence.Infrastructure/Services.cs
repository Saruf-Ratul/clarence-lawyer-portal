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
        new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), ClientId = Clients[0].Id, AddressLine1 = "456 Oak Ave", City = "Jersey City", PostalCode = "07302" },
        new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222223"), ClientId = Clients[1].Id, AddressLine1 = "9 South St", City = "Paterson", PostalCode = "07501" }
    ];

    public static readonly List<Tenant> Tenants =
    [
        new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333331"), FullName = "John Doe", Unit = "2B" },
        new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333332"), FullName = "Maria Santos", Unit = "4A" },
        new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), FullName = "David Kim", Unit = "1C" }
    ];

    public static readonly List<LegalCase> Cases =
    [
        new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444441"), CaseNumber = "CL-2026-1184", ClientId = Clients[0].Id, PropertyId = Properties[0].Id, TenantId = Tenants[0].Id, MonthlyRent = 1800, BalanceOwed = 5400, LateFees = 150, CourtCosts = 75, Stage = CaseStage.Accepted, NextDeadline = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(4)) },
        new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444442"), CaseNumber = "CL-2026-1185", ClientId = Clients[0].Id, PropertyId = Properties[1].Id, TenantId = Tenants[1].Id, MonthlyRent = 2200, BalanceOwed = 4400, LateFees = 100, CourtCosts = 75, Stage = CaseStage.Filed, NextDeadline = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(9)) }
    ];

    public static readonly List<RentManagerSubmission> Submissions =
    [
        new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555551"), ClientId = Clients[1].Id, PropertyId = Properties[2].Id, TenantId = Tenants[2].Id, MonthlyRent = 1700, BalanceOwed = 5100, ReceivedAtUtc = DateTime.UtcNow.AddHours(-7), VerificationStatus = "1:1 RM Match" }
    ];

    public static readonly List<CaseTimelineEvent> Timeline =
    [
        new() { Id = Guid.NewGuid(), LegalCaseId = Cases[0].Id, AtUtc = DateTime.UtcNow.AddDays(-3), Stage = CaseStage.Accepted, Actor = "Atty Rivera", Notes = "Case accepted from RM intake" },
        new() { Id = Guid.NewGuid(), LegalCaseId = Cases[1].Id, AtUtc = DateTime.UtcNow.AddDays(-2), Stage = CaseStage.Filed, Actor = "Paralegal Team", Notes = "Filed on NJ Courts" }
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
        => Task.FromResult((IReadOnlyList<RmSubmissionDto>)FakeStore.Submissions.Where(s => !s.Accepted).Select(s =>
        {
            var c = FakeStore.Clients.First(x => x.Id == s.ClientId);
            var p = FakeStore.Properties.First(x => x.Id == s.PropertyId);
            var t = FakeStore.Tenants.First(x => x.Id == s.TenantId);
            return new RmSubmissionDto(s.Id, c.Name, $"{p.AddressLine1}, {p.City}", t.FullName, t.Unit, s.BalanceOwed, s.MonthlyRent, s.ReceivedAtUtc, s.VerificationStatus);
        }).ToList());

    public Task<CaseDetailDto?> AcceptAsync(AcceptSubmissionRequest request, CancellationToken cancellationToken = default)
    {
        var sub = FakeStore.Submissions.FirstOrDefault(x => x.Id == request.SubmissionId && !x.Accepted);
        if (sub is null) return Task.FromResult<CaseDetailDto?>(null);
        sub.Accepted = true;
        var legalCase = new LegalCase { Id = Guid.NewGuid(), CaseNumber = $"CL-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(2000, 9999)}", ClientId = sub.ClientId, PropertyId = sub.PropertyId, TenantId = sub.TenantId, MonthlyRent = sub.MonthlyRent, BalanceOwed = sub.BalanceOwed, Stage = CaseStage.Accepted, NextDeadline = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)) };
        FakeStore.Cases.Add(legalCase);
        FakeStore.Timeline.Add(new CaseTimelineEvent { Id = Guid.NewGuid(), LegalCaseId = legalCase.Id, AtUtc = DateTime.UtcNow, Stage = CaseStage.Accepted, Actor = request.AcceptedBy, Notes = "Accepted from RM Intake Inbox" });
        return new CaseService().GetCaseAsync(legalCase.Id, cancellationToken);
    }
}
