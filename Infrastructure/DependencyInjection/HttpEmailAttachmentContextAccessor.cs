using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MiApp.DTOs.DTOs.Email.Core;
using MiApp.Services.Service.Email.Attachments;

namespace DocuArchi.Api.Infrastructure.DependencyInjection;

public sealed class HttpEmailAttachmentContextAccessor(IHttpContextAccessor http) : IEmailAttachmentContextAccessor
{
    public EmailAttachmentContext Get(SolicitudEnvioCorreoDto request, string alias, int companyId)
    {
        var user = http.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true
            || !int.TryParse(user.FindFirstValue("emp"), out var company) || company != companyId
            || !int.TryParse(user.FindFirstValue("usuarioid") ?? user.FindFirstValue("uid"), out var userId) || userId <= 0
            || !string.Equals(user.FindFirstValue("defaulalias")?.Trim(), alias, StringComparison.Ordinal))
            throw new EmailAttachmentException("EMAIL_ATTACHMENT_ACCESS_DENIED", true);
        return new(company, userId, alias, request.CodigoProceso.Trim(), request.EntidadNegocio.Trim(), request.IdEntidadNegocio.Trim());
    }
}
