using MiApp.DTOs.DTOs.GestorDocumental.Expedientes;
using MiApp.DTOs.DTOs.Utilidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DocuArchi.Api.Infrastructure.Filters;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ExpedienteAutocompleteValidationResponseAttribute : ActionFilterAttribute
{
    public ExpedienteAutocompleteValidationResponseAttribute()
    {
        Order = -3000;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid) return;

        var requestId = context.HttpContext.TraceIdentifier;
        context.Result = new BadRequestObjectResult(new AppResponses<ExpedienteAutocompleteResponseDto>
        {
            success = false,
            message = "EXPEDIENTE_AUTOCOMPLETE_REQUEST_INVALID",
            data = null!,
            meta = new AppMeta { Status = "error", RequestId = requestId },
            errors =
            [
                new
                {
                    Code = "EXPEDIENTE_AUTOCOMPLETE_REQUEST_INVALID",
                    UserMessage = "Campo, texto o límite inválido.",
                    RequestId = requestId
                }
            ]
        });
    }
}
