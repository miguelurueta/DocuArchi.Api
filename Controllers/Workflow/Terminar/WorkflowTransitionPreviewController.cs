using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.DTOs.DTOs.Workflow.Terminar;
using MiApp.Services.Service.Workflow.Terminar;
using MiApp.Services.Service.Workflow.Terminar.Errors;

namespace DocuArchi.Api.Controllers.Workflow.Terminar;

[ApiController]
[Authorize]
[Route("api/workflow/tareas")]
public sealed class WorkflowTransitionPreviewController(
    IConsultarTransicionesDisponiblesService service) : ControllerBase
{
    [HttpGet("{idTarea:long}/transiciones")]
    [ProducesResponseType(typeof(AppResponses<WorkflowTransitionPreviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(AppResponses<WorkflowTransitionPreviewDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponses<WorkflowTransitionPreviewDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(AppResponses<WorkflowTransitionPreviewDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponses<WorkflowTransitionPreviewDto>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(AppResponses<WorkflowTransitionPreviewDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AppResponses<WorkflowTransitionPreviewDto>>> GetAsync(
        long idTarea,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.ConsultarAsync(idTarea, cancellationToken).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return Ok(new AppResponses<WorkflowTransitionPreviewDto>
                {
                    success = true,
                    message = "OK",
                    data = result.Data!,
                    errors = [],
                    meta = new AppMeta
                    {
                        Total = result.Data!.Transiciones.Count,
                        Status = "success",
                        RequestId = RequestId(),
                        ResponseVersion = "1.0"
                    }
                });
            }

            var descriptor = WorkflowPreviewErrorCatalog.Get(result.Status);
            return StatusCode(
                descriptor.HttpStatus,
                WorkflowPreviewErrorResponseFactory.Create<WorkflowTransitionPreviewDto>(result, RequestId()));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            var result = new WorkflowPreviewServiceResult(WorkflowPreviewServiceStatus.UnexpectedFailure);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                WorkflowPreviewErrorResponseFactory.Create<WorkflowTransitionPreviewDto>(result, RequestId()));
        }
    }

    private string RequestId()
    {
        var value = Activity.Current?.TraceId.ToString()
            ?? HttpContext?.TraceIdentifier;
        return IsSafeRequestId(value) ? value! : Guid.NewGuid().ToString("N");
    }

    private static bool IsSafeRequestId(string? value) =>
        value is { Length: >= 8 and <= 96 }
        && value.All(character => char.IsAsciiLetterOrDigit(character) || character is '_' or '-');
}
