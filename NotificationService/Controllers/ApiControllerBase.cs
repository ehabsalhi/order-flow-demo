using Microsoft.AspNetCore.Mvc;
using NotificationService.DTOs.Common;

namespace NotificationService.Controllers;

[ApiController]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(ApiErrorResponse))]
public abstract class ApiControllerBase : ControllerBase;
