namespace Clarence.Application;

public record DashboardKpiDto(int ActiveClients, int UniqueProperties, int OpenCases, int NewSubmissions);

public record ClientSummaryDto(Guid Id, string Name, int Properties, int OpenCases, string RmSyncStatus);
public record PropertySummaryDto(Guid Id, Guid ClientId, string Address, int OpenCases, string RmSyncStatus);
public record CaseSummaryDto(Guid Id, string CaseNumber, string ClientName, string PropertyAddress, string TenantName, string Stage, decimal BalanceOwed, DateOnly? NextDeadline);
public record RmSubmissionDto(Guid Id, string ClientName, string PropertyAddress, string TenantName, string Unit, decimal BalanceOwed, decimal MonthlyRent, DateTime ReceivedAtUtc, string VerificationStatus);
public record TimelineEventDto(DateTime AtUtc, string Stage, string Actor, string Notes);

public record CaseDetailDto(
    Guid Id,
    string CaseNumber,
    string ClientName,
    string PropertyAddress,
    string TenantName,
    string Unit,
    decimal MonthlyRent,
    decimal BalanceOwed,
    decimal LateFees,
    decimal CourtCosts,
    DateOnly? NextDeadline,
    string Stage,
    IReadOnlyList<TimelineEventDto> Timeline);

public record UpdateCaseStatusRequest(string Stage, string Notes);
public record AcceptSubmissionRequest(Guid SubmissionId, string AcceptedBy);

public interface IDashboardService { Task<DashboardKpiDto> GetKpisAsync(CancellationToken cancellationToken = default); }
public interface IClientService { Task<IReadOnlyList<ClientSummaryDto>> GetClientsAsync(CancellationToken cancellationToken = default); }
public interface IPropertyService { Task<IReadOnlyList<PropertySummaryDto>> GetPropertiesByClientAsync(Guid clientId, CancellationToken cancellationToken = default); }
public interface ICaseService
{
    Task<IReadOnlyList<CaseSummaryDto>> GetCasesAsync(CancellationToken cancellationToken = default);
    Task<CaseDetailDto?> GetCaseAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<CaseDetailDto?> UpdateStatusAsync(Guid caseId, UpdateCaseStatusRequest request, CancellationToken cancellationToken = default);
}
public interface IRentManagerInboxService
{
    Task<IReadOnlyList<RmSubmissionDto>> GetSubmissionsAsync(CancellationToken cancellationToken = default);
    Task<CaseDetailDto?> AcceptAsync(AcceptSubmissionRequest request, CancellationToken cancellationToken = default);
}
