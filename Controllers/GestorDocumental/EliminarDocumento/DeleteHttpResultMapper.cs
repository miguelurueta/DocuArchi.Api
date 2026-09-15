using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Models.Models.GestorDocumental.EliminarDocumento;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestorDocumental.EliminarDocumento
{
    internal static class DeleteHttpResultMapper
    {
        internal static ActionResult<AppResponses<StorageDeleteResult?>> Map(
            ControllerBase controller,
            int statusCode,
            AppResponses<StorageDeleteResult?> response)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => controller.BadRequest(response),
                StatusCodes.Status404NotFound => controller.NotFound(response),
                _ => controller.StatusCode(statusCode, response)
            };
        }
    }
}
