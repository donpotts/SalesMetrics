using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using AppSalesMetrics.Data;
using AppSalesMetrics.Shared.Models;

namespace AppSalesMetrics.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting("Fixed")]
public class SalesMetricController(ApplicationDbContext ctx) : ControllerBase
{
    [HttpGet("")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<IQueryable<SalesMetric>> Get()
    {
        return Ok(ctx.SalesMetric);
    }

    [HttpGet("{key}")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SalesMetric>> GetAsync(long key)
    {
        var salesMetric = await ctx.SalesMetric.FirstOrDefaultAsync(x => x.Id == key);

        if (salesMetric == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(salesMetric);
        }
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SalesMetric>> PostAsync(SalesMetric salesMetric)
    {
        var record = await ctx.SalesMetric.FindAsync(salesMetric.Id);
        if (record != null)
        {
            return Conflict();
        }
    
        await ctx.SalesMetric.AddAsync(salesMetric);

        await ctx.SaveChangesAsync();

        return Created($"/salesmetric/{salesMetric.Id}", salesMetric);
    }

    [HttpPut("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SalesMetric>> PutAsync(long key, SalesMetric update)
    {
        var salesMetric = await ctx.SalesMetric.FirstOrDefaultAsync(x => x.Id == key);

        if (salesMetric == null)
        {
            return NotFound();
        }

        ctx.Entry(salesMetric).CurrentValues.SetValues(update);

        await ctx.SaveChangesAsync();

        return Ok(salesMetric);
    }

    [HttpPatch("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SalesMetric>> PatchAsync(long key, Delta<SalesMetric> delta)
    {
        var salesMetric = await ctx.SalesMetric.FirstOrDefaultAsync(x => x.Id == key);

        if (salesMetric == null)
        {
            return NotFound();
        }

        delta.Patch(salesMetric);

        await ctx.SaveChangesAsync();

        return Ok(salesMetric);
    }

    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(long key)
    {
        var salesMetric = await ctx.SalesMetric.FindAsync(key);

        if (salesMetric != null)
        {
            ctx.SalesMetric.Remove(salesMetric);
            await ctx.SaveChangesAsync();
        }

        return NoContent();
    }
}
