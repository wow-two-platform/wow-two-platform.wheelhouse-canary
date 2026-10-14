using Microsoft.AspNetCore.Mvc;
using WheelhouseCanary.Api.Pings.Requests;
using WheelhouseCanary.Application.Pings.Models;
using WheelhouseCanary.Application.Pings.Queries;
using WoW.Two.Sdk.Backend.Beta.Mediator;
using WoW.Two.Sdk.Backend.Beta.Web.Contracts;
using WoW.Two.Sdk.Backend.Beta.Web.ExceptionHandling.Extensions;

namespace WheelhouseCanary.Api.Pings.Controllers;

/// <summary>Exposes pings over HTTP.</summary>
[ApiController]
[Route("api/pings")]
public sealed class PingsController(ISender sender) : ControllerBase
{
    /// <summary>Gets the latest pings, newest first.</summary>
    [HttpGet]
    [ProducesResponseType<ApiResponse<IReadOnlyList<PingDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await sender.SendAsync(new PingListQuery(), ct);

        return result.Match<IActionResult>(
            ok => Ok(ApiResponse<IReadOnlyList<PingDto>>.Ok(ok.Data)),
            fail => this.ToProblem(fail.Error));
    }

    /// <summary>Records a ping.</summary>
    [HttpPost]
    [ProducesResponseType<ApiResponse<PingDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Add([FromBody] PingAddApiRequest request, CancellationToken ct)
    {
        var result = await sender.SendAsync(request.ToCommand(), ct);

        return result.Match<IActionResult>(
            ok => StatusCode(StatusCodes.Status201Created, ApiResponse<PingDto>.Ok(ok.Data)),
            fail => this.ToProblem(fail.Error));
    }
}
