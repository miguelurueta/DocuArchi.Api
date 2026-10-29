using MiApp.Models.Models.DocumentProcessing.IntegracionAlmacenamientoContextoSolicitud;
using MiApp.Repository.Repositorio.DataAccess;

namespace DocuArchi.Api.Infrastructure.HostedServices;

internal static class StorageProcessingWorkerAliases
{
    public static string[] Resolve(IServiceScopeFactory scopes, string defaultAlias, StorageProcessingOptions integration, StorageProcessingDispatchOptions dispatch)
    {
        if (!integration.Enabled || !dispatch.Enabled) return string.IsNullOrWhiteSpace(defaultAlias) ? [] : [defaultAlias];
        using var scope = scopes.CreateScope();
        var available = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>().GetAvailableAliases().ToHashSet(StringComparer.OrdinalIgnoreCase);
        // Several aliases can point at one base: visit each scope once, preferring
        // the preexisting host alias when it belongs to that configured base.
        var selected = integration.BaseScopes.Where(x => available.Contains(x.Key) && !string.IsNullOrWhiteSpace(x.Value))
            .GroupBy(x => x.Value, StringComparer.Ordinal)
            .Select(g => g.OrderByDescending(x => string.Equals(x.Key, defaultAlias, StringComparison.OrdinalIgnoreCase)).First().Key).ToList();
        if (!string.IsNullOrWhiteSpace(defaultAlias) && available.Contains(defaultAlias)
            && !integration.BaseScopes.ContainsKey(defaultAlias)) selected.Insert(0, defaultAlias);
        return selected.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }
}
