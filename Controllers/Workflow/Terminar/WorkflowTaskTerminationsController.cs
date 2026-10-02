using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AutoMapper;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.DTOs.DTOs.Workflow.Terminar;
using MiApp.Models.Models.Workflow.Terminar;
using MiApp.Services.Service.Mapping.Workflow.Terminar;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using MiApp.Services.Service.Workflow.Terminar;
using MiApp.Services.Service.Workflow.Terminar.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.Workflow.Terminar;

/// <summary>
/// Completes a Workflow task through the atomic Normal/Final transition use case.
/// Server-owned routing, authorization and destination facts are resolved by
/// the application service and are intentionally absent from the HTTP input.
/// </summary>
[Route("api/workflow/tareas")]
[ApiController]
[Authorize]
public sealed class WorkflowTaskTerminationsController : ControllerBase
{
    private readonly IClaimValidationService _claimValidationService;
    private readonly IMapper _mapper;
    private readonly ICompleteWorkflowTaskService _service;
    private readonly ILogger<WorkflowTaskTerminationsController> _logger;

    public WorkflowTaskTerminationsController(
        IClaimValidationService claimValidationService,
        IMapper mapper,
        ICompleteWorkflowTaskService service,
        ILogger<WorkflowTaskTerminationsController> logger)
    {
        _claimValidationService = claimValidationService;
        _mapper = mapper;
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Executes one server-classified Normal or Final transition. Reusing an
    /// idempotency key with the same authorized payload returns the committed result.
    /// </summary>
    [HttpPost("{idTarea:long}/terminaciones")]
    [ProducesResponseType(typeof(AppResponses<CompleteWorkflowTaskResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponses<CompleteWorkflowTaskResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponses<CompleteWorkflowTaskResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(AppResponses<CompleteWorkflowTaskResponseDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(AppResponses<CompleteWorkflowTaskResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponses<CompleteWorkflowTaskResponseDto>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(AppResponses<CompleteWorkflowTaskResponseDto>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(AppResponses<CompleteWorkflowTaskResponseDto>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(AppResponses<CompleteWorkflowTaskResponseDto>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<AppResponses<CompleteWorkflowTaskResponseDto>>> CompleteAsync(
        [FromRoute] long idTarea,
        [FromBody] CompleteWorkflowTaskRequestDto request,
        [FromHeader(Name = WorkflowTerminationHeaders.IdempotencyKey)] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var requestId = Guid.NewGuid().ToString("N");

        try
        {
            if (idTarea <= 0 || request is null || !HasValidSelection(request))
            {
                return FromFailure(CompleteWorkflowTaskServiceStatus.InvalidRequest, requestId, "request");
            }

            var normalizedIdempotencyKey = idempotencyKey?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedIdempotencyKey))
            {
                return FromFailure(
                    CompleteWorkflowTaskServiceStatus.InvalidRequest,
                    requestId,
                    WorkflowTerminationHeaders.IdempotencyKey);
            }

            var userValidation = _claimValidationService.ValidateClaim<string>("IdUsuarioWorkflow");
            if (!userValidation.Success
                || !int.TryParse(userValidation.ClaimValue, out var workflowUserId)
                || workflowUserId <= 0)
            {
                return FromFailure(CompleteWorkflowTaskServiceStatus.AccessDenied, requestId, "IdUsuarioWorkflow");
            }

            var companyValidation = _claimValidationService.ValidateClaim<string>("emp");
            if (!companyValidation.Success
                || !int.TryParse(companyValidation.ClaimValue, out var companyId)
                || companyId <= 0)
            {
                return FromFailure(CompleteWorkflowTaskServiceStatus.AccessDenied, requestId, "emp");
            }

            var idempotency = new WorkflowIdempotencyKey(
                idTarea,
                workflowUserId,
                WorkflowTransitionOperation.Normal,
                normalizedIdempotencyKey);
            var payloadHash = CreatePayloadHash(request);
            var command = _mapper.Map<CompleteWorkflowTaskCommand>(
                request,
                options =>
                {
                    options.Items[WorkflowTerminationMappingContextKeys.TaskId] = idTarea;
                    options.Items[WorkflowTerminationMappingContextKeys.WorkflowUserId] = workflowUserId;
                    options.Items[WorkflowTerminationMappingContextKeys.CompanyId] = companyId;
                    options.Items[WorkflowTerminationMappingContextKeys.IdempotencyKey] = idempotency;
                    options.Items[WorkflowTerminationMappingContextKeys.PayloadHash] = payloadHash;
                });

            var result = await _service.CompleteAsync(command, cancellationToken).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                LogCompletion(requestId, idTarea, workflowUserId, result.Status);
                return FromResultFailure(result, requestId);
            }

            var mappedResult = _mapper.Map<CompleteWorkflowTaskResponseDto>(result.Result!);
            var hasNotificationWarning =
                result.Result!.Notification.Status == WorkflowTaskNotificationStatus.Warning;
            var response = new AppResponses<CompleteWorkflowTaskResponseDto>
            {
                success = true,
                message = hasNotificationWarning
                    ? "La tarea Workflow fue terminada con una alarma de notificación."
                    : "La tarea Workflow fue terminada.",
                data = mappedResult,
                errors = [],
                meta = new AppMeta
                {
                    Status = hasNotificationWarning
                        ? "completed_with_notification_warning"
                        : result.Status == CompleteWorkflowTaskServiceStatus.Replayed ? "replayed" : "completed",
                    RequestId = requestId,
                    ResponseVersion = "1.0"
                }
            };

            LogCompletion(requestId, idTarea, workflowUserId, result.Status);
            return Ok(response);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            _logger.LogError(
                "Workflow termination failed unexpectedly. stage={Stage} requestId={RequestId} taskId={TaskId} errorCode={ErrorCode}",
                "Controller",
                requestId,
                idTarea,
                WorkflowTerminationErrorCatalog.Unexpected);
            return FromFailure(CompleteWorkflowTaskServiceStatus.UnexpectedFailure, requestId);
        }
    }

    private ActionResult<AppResponses<CompleteWorkflowTaskResponseDto>> FromResultFailure(
        CompleteWorkflowTaskServiceResult result,
        string requestId) =>
        StatusCode(
            WorkflowTerminationErrorCatalog.Get(result.Status).HttpStatus,
            WorkflowTerminationErrorResponseFactory.Create<CompleteWorkflowTaskResponseDto>(result, requestId));

    private ActionResult<AppResponses<CompleteWorkflowTaskResponseDto>> FromFailure(
        CompleteWorkflowTaskServiceStatus status,
        string requestId,
        string? field = null)
    {
        var result = CompleteWorkflowTaskServiceResult.Failure(status);
        return StatusCode(
            WorkflowTerminationErrorCatalog.Get(status).HttpStatus,
            WorkflowTerminationErrorResponseFactory.Create<CompleteWorkflowTaskResponseDto>(result, requestId, field));
    }

    private void LogCompletion(
        string requestId,
        long taskId,
        int workflowUserId,
        CompleteWorkflowTaskServiceStatus status) =>
        _logger.LogInformation(
            "Workflow termination completed. stage={Stage} requestId={RequestId} taskId={TaskId} workflowUserId={WorkflowUserId} status={Status}",
            "Controller",
            requestId,
            taskId,
            workflowUserId,
            status);

    private static bool HasValidSelection(CompleteWorkflowTaskRequestDto request) =>
        request.TokenVersion > 0
        && ((request.IdConector is > 0
                && string.IsNullOrWhiteSpace(request.PolicyVersion)
                && string.IsNullOrWhiteSpace(request.ManagedDestinationPreviewToken))
            || (!request.IdConector.HasValue
                && !string.IsNullOrWhiteSpace(request.PolicyVersion)
                && !string.IsNullOrWhiteSpace(request.ManagedDestinationPreviewToken)));

    private static WorkflowPayloadHash CreatePayloadHash(CompleteWorkflowTaskRequestDto request)
    {
        var canonicalPayload = JsonSerializer.SerializeToUtf8Bytes(new
        {
            request.TokenVersion,
            request.IdConector,
            request.PolicyVersion,
            request.ManagedDestinationPreviewToken
        });
        return new WorkflowPayloadHash(Convert.ToHexString(SHA256.HashData(canonicalPayload)));
    }
}
