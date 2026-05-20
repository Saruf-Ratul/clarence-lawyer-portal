using Clarence.Application;

namespace Clarence.Infrastructure;

public class DashboardService : IDashboardService
{
    public Task<DashboardKpiDto> GetKpisAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(new DashboardKpiDto(12, 28, 47, 4));
}
