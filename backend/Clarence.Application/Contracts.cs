namespace Clarence.Application;

public record DashboardKpiDto(int ActiveClients, int UniqueProperties, int OpenCases, int NewSubmissions);

public interface IDashboardService
{
    Task<DashboardKpiDto> GetKpisAsync(CancellationToken cancellationToken = default);
}
