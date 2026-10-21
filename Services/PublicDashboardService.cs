using Microsoft.EntityFrameworkCore;
using ProxyCraftPlus.Api.Contracts;
using ProxyCraftPlus.Data;
using ProxyCraftPlus.Models;

namespace ProxyCraftPlus.Services;

public sealed class PublicDashboardService(
    SettingsStore settings,
    ProxyCraftPlusDbContext database)
{
    public async Task<PublicDashboardResponse?> GetAsync(CancellationToken cancellationToken, bool preview = false)
    {
        if (!settings.Current.PublicDashboardEnabled && !preview)
        {
            return null;
        }

        IReadOnlyList<string> domains = settings.Current.PublicDashboardEnabled && settings.Current.ShowPublicDomains
            ? await database.Proxies.AsNoTracking()
                .Where((Proxy proxy) => proxy.IsActive)
                .OrderBy((Proxy proxy) => proxy.Domain)
                .Select((Proxy proxy) => proxy.Domain)
                .ToListAsync(cancellationToken)
            : [];

        return new PublicDashboardResponse(domains, settings.Current.PublicDashboardMessage, settings.Current.PublicDashboardTitle);
    }
}
