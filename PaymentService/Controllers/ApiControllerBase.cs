using Microsoft.AspNetCore.Mvc;
using PaymentService.DTOs.Common;

namespace PaymentService.Controllers;

[ApiController]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(ApiErrorResponse))]
public abstract class ApiControllerBase : ControllerBase;
