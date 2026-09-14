using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DocuArchi.Api.Infrastructure.Observability;

public interface IRequestCorrelationContext
{
    string RequestId { get; }
}

public sealed partial class RequestCorrelationContext : IRequestCorrelationContext
{
    private const string HeaderName = "X-Correlation-ID";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private string? _requestId;

    public RequestCorrelationContext(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    public string RequestId => _requestId ??= Resolve();

    private string Resolve()
    {
        var context = _httpContextAccessor.HttpContext;
        var candidate = context?.Request.Headers[HeaderName].FirstOrDefault()?.Trim();
        var requestId = candidate is not null && SafeCorrelationId().IsMatch(candidate)
            ? candidate
            : Activity.Current?.TraceId.ToString();

        if (string.IsNullOrWhiteSpace(requestId) || requestId.All(x => x == '0'))
            requestId = Guid.NewGuid().ToString("N");

        if (context is not null)
        {
            context.TraceIdentifier = requestId;
            context.Response.Headers[HeaderName] = requestId;
        }

        return requestId;
    }

    [GeneratedRegex("^[A-Za-z0-9._-]{1,128}$", RegexOptions.CultureInvariant)]
    private static partial Regex SafeCorrelationId();
}
