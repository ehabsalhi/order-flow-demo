using MainServer.DTOs.Common;
using Microsoft.AspNetCore.Mvc;

namespace MainServer.Controllers;

[ApiController]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(ApiErrorResponse))]
public abstract class ApiControllerBase : ControllerBase;
