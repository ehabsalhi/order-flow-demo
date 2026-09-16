using MainServer.DTOs.Common;
using MainServer.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MainServer.Controllers;

[ApiController]
[EnableRateLimiting(RateLimitPolicies.Api)]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(ApiErrorResponse))]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status429TooManyRequests)]
public abstract class ApiControllerBase : ControllerBase;
