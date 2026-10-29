using MiApp.DTOs.DTOs.DocumentProcessing.Core;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.DocumentProcessing.Core;
using MiApp.Services.Service.SessionHelper;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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
            var clock=Stopwatch.StartNew();
            var requestId=HttpContext.TraceIdentifier;
            var publicCode="";
            var metaStatus="success";
            try
            {
                var alias = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!alias.Success || string.IsNullOrWhiteSpace(alias.ClaimValue))
                {
                    publicCode="DOCPROC_ALIAS_REQUIRED";metaStatus="error";
                    return BadRequest(DocumentProcessingErrorResponseFactory.Build<DocumentProcessingJobStatusDto?>(publicCode,requestId));
                }

                var result = await _service.GetJobAsync(jobId, alias.ClaimValue, cancellationToken);
                if (result == null)
                {
                    publicCode="DOCPROC_JOB_NOT_FOUND";metaStatus="error";
                    return NotFound(DocumentProcessingErrorResponseFactory.Build<DocumentProcessingJobStatusDto?>(publicCode,requestId));
                }

                return Ok(new AppResponses<DocumentProcessingJobStatusDto?>
                {
                    success = true,
                    message = "OK",
                    data = ToDto(result),
                    meta = DocumentProcessingErrorResponseFactory.Metadata(requestId,result),
                    errors = Array.Empty<object>()
                });
            }
            catch (Exception ex)
            {
                publicCode="DOCPROC_GET_FAILED";metaStatus="error";
                var descriptor=DocumentProcessingErrorCatalog.Resolve(publicCode);
                return StatusCode(descriptor.HttpStatus,DocumentProcessingErrorResponseFactory.Build<DocumentProcessingJobStatusDto?>(publicCode,requestId,ex));
            }
            finally {
                try {
                    var actor=_currentUserService.UsuarioIdInt;
                    _logger.LogInformation("docproc_get requestId={RequestId} docprocJobId={DocprocJobId} stage={Stage} operation={Operation} errorCode={ErrorCode} metaStatus={MetaStatus} durationMs={DurationMs} usuarioId={UsuarioId}",
                        requestId,jobId,"lookup","GetJob",publicCode,metaStatus,clock.ElapsedMilliseconds,actor>0?(int?)actor:null);
                }
                catch { /* A logging/optional actor failure must not replace the primary HTTP result. */ }
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
