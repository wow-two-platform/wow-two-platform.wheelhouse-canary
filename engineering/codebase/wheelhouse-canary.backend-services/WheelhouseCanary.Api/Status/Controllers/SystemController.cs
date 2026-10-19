using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WheelhouseCanary.Api.Status.Models;
using WoW.Two.Sdk.Beta.Be.Web.Contracts;

namespace WheelhouseCanary.Api.Status.Controllers;

/// <summary>Exposes system status over HTTP.</summary>
[ApiController]
[Route("api/system")]
public sealed class SystemController(IOptions<CanarySettings> settings) : ControllerBase
{
    /// <summary>Holds the name the service reports.</summary>
    private const string ServiceName = "wheelhouse-canary";

    /// <summary>Reports service liveness.</summary>
    /// <returns>The service name, liveness state, running version and environment label.</returns>
    [HttpGet("status")]
    [AllowAnonymous]
    [ProducesResponseType<ApiResponse<SystemStatusDto>>(StatusCodes.Status200OK)]
    public IActionResult Status()
    {
        var version = typeof(SystemController).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
        var status = new SystemStatusDto
        {
            Service = ServiceName,
            Status = "ok",
            Version = version,
            Label = settings.Value.Label,
        };

        return Ok(ApiResponse<SystemStatusDto>.Ok(status));
    }
}
