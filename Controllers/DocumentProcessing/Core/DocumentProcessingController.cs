using MiApp.DTOs.DTOs.DocumentProcessing.Core;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.DocumentProcessing.Core;
using MiApp.Services.Service.SessionHelper;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.DocumentProcessing.Core
{
    [Route("api/document-processing/jobs")]
    [ApiController]
    [Authorize]
    public sealed class DocumentProcessingController : ControllerBase
    {
        private readonly IDocumentProcessingService _service;
        private readonly IClaimValidationService _claimValidationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DocumentProcessingController> _logger;

        public DocumentProcessingController(
            IDocumentProcessingService service,
            IClaimValidationService claimValidationService,
            ICurrentUserService currentUserService,
            ILogger<DocumentProcessingController> logger)
        {
            _service = service;
            _claimValidationService = claimValidationService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(AppResponses<DocumentProcessingJobAcceptedDto?>), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(AppResponses<DocumentProcessingJobAcceptedDto?>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AppResponses<DocumentProcessingJobAcceptedDto?>>> CreateJob(
            [FromBody] CreateDocumentProcessingJobRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var alias = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!alias.Success || string.IsNullOrWhiteSpace(alias.ClaimValue))
                {
                    return BadRequest(Error<DocumentProcessingJobAcceptedDto?>("DOCPROC_ALIAS_REQUIRED", "Claim defaulalias requerido."));
                }

                var usuarioId = _currentUserService.ResolveUsuarioIdOrThrow();
                var result = await _service.CreateJobAsync(ToServiceRequest(request, alias.ClaimValue, usuarioId), cancellationToken);

                return StatusCode(StatusCodes.Status202Accepted, new AppResponses<DocumentProcessingJobAcceptedDto?>
                {
                    success = true,
                    message = "Accepted",
                    data = new DocumentProcessingJobAcceptedDto
                    {
                        JobId = result.JobId,
                        Status = result.Status,
                        ProcessorType = result.ProcessorType,
                        Accepted = result.Accepted,
                        StatusUrl = result.StatusUrl
                    },
                    errors = Array.Empty<object>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "docproc_create_failed errorCode={ErrorCode}", "DOCPROC_CREATE_FAILED");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    Error<DocumentProcessingJobAcceptedDto?>("DOCPROC_CREATE_FAILED", "No fue posible crear el job de procesamiento documental."));
            }
        }

        [HttpGet("{jobId:long}")]
        [ProducesResponseType(typeof(AppResponses<DocumentProcessingJobStatusDto?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<DocumentProcessingJobStatusDto?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AppResponses<DocumentProcessingJobStatusDto?>>> GetJob(
            [FromRoute] long jobId,
            CancellationToken cancellationToken)
        {
            try
            {
                var alias = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!alias.Success || string.IsNullOrWhiteSpace(alias.ClaimValue))
                {
                    return BadRequest(Error<DocumentProcessingJobStatusDto?>("DOCPROC_ALIAS_REQUIRED", "Claim defaulalias requerido."));
                }

                var result = await _service.GetJobAsync(jobId, alias.ClaimValue, cancellationToken);
                if (result == null)
                {
                    return NotFound(Error<DocumentProcessingJobStatusDto?>("DOCPROC_JOB_NOT_FOUND", "Job de procesamiento documental no encontrado."));
                }

                return Ok(new AppResponses<DocumentProcessingJobStatusDto?>
                {
                    success = true,
                    message = "OK",
                    data = ToDto(result),
                    errors = Array.Empty<object>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "docproc_get_failed jobId={JobId} errorCode={ErrorCode}", jobId, "DOCPROC_GET_FAILED");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    Error<DocumentProcessingJobStatusDto?>("DOCPROC_GET_FAILED", "No fue posible consultar el job de procesamiento documental."));
            }
        }

        private static DocumentProcessingCreateRequest ToServiceRequest(
            CreateDocumentProcessingJobRequestDto request,
            string defaultDbAlias,
            int usuarioId)
        {
            return new DocumentProcessingCreateRequest
            {
                Source = request.Source,
                RutaTemporalId = request.RutaTemporalId,
                ArchivoTemporalId = request.ArchivoTemporalId,
                StoredDocument = request.StoredDocument == null
                    ? null
                    : new StoredDocumentReference
                    {
                        Gabinete = request.StoredDocument.Gabinete,
                        IdDocumento = request.StoredDocument.IdDocumento,
                        Alias = request.StoredDocument.Alias,
                        StableSourceId = request.StoredDocument.StableSourceId
                    },
                FilePath = request.FilePath,
                StableSourceId = request.StableSourceId ?? request.StoredDocument?.StableSourceId,
                RequestedProfileKey = request.RequestedProfileKey,
                RequestedOutput = request.RequestedOutput,
                Priority = request.Priority,
                Metadata = request.Metadata,
                DefaultDbAlias = defaultDbAlias,
                UsuarioId = usuarioId,
                Usuario = usuarioId.ToString()
            };
        }

        private static DocumentProcessingJobStatusDto ToDto(DocumentProcessingJobStatus result)
        {
            return new DocumentProcessingJobStatusDto
            {
                JobId = result.JobId,
                Status = result.Status,
                ProcessorType = result.ProcessorType,
                ProcessorJobId = result.ProcessorJobId,
                StableSourceId = result.StableSourceId,
                DeliveryTypes = result.DeliveryTypes,
                Quality = result.Quality == null
                    ? null
                    : new DocumentProcessingQualityDto
                    {
                        TextLength = result.Quality.TextLength,
                        MeanConfidence = result.Quality.MeanConfidence,
                        RequiresReview = result.Quality.RequiresReview
                    },
                Artifacts = result.Artifacts.Select(x => new DocumentProcessingArtifactDto
                {
                    ArtifactId = x.ArtifactId,
                    Type = x.Type,
                    AccessPolicy = x.AccessPolicy
                }).ToList(),
                Errors = result.Errors.Select(x => new DocumentProcessingErrorDto
                {
                    Code = x.Code,
                    UserMessage = x.UserMessage
                }).ToList()
            };
        }

        private static AppResponses<T> Error<T>(string code, string message)
        {
            return new AppResponses<T>
            {
                success = false,
                message = message,
                data = default,
                errors = new[] { new { Code = code, UserMessage = message } }
            };
        }
    }
}
