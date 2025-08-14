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
public class OrderItemController(ApplicationDbContext ctx) : ControllerBase
{
    [HttpGet("")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<IQueryable<OrderItem>> Get()
    {
        return Ok(ctx.OrderItem.Include(x => x.Product));
    }

    [HttpGet("{key}")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderItem>> GetAsync(long key)
    {
        var orderItem = await ctx.OrderItem.Include(x => x.Product).FirstOrDefaultAsync(x => x.Id == key);

        if (orderItem == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(orderItem);
        }
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderItem>> PostAsync(OrderItem orderItem)
    {
        var record = await ctx.OrderItem.FindAsync(orderItem.Id);
        if (record != null)
        {
            return Conflict();
        }
    
        await ctx.OrderItem.AddAsync(orderItem);

        await ctx.SaveChangesAsync();

        return Created($"/orderitem/{orderItem.Id}", orderItem);
    }

    [HttpPut("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderItem>> PutAsync(long key, OrderItem update)
    {
        var orderItem = await ctx.OrderItem.FirstOrDefaultAsync(x => x.Id == key);

        if (orderItem == null)
        {
            return NotFound();
        }

        ctx.Entry(orderItem).CurrentValues.SetValues(update);

        await ctx.SaveChangesAsync();

        return Ok(orderItem);
    }

    [HttpPatch("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderItem>> PatchAsync(long key, Delta<OrderItem> delta)
    {
        var orderItem = await ctx.OrderItem.FirstOrDefaultAsync(x => x.Id == key);

        if (orderItem == null)
        {
            return NotFound();
        }

        delta.Patch(orderItem);

        await ctx.SaveChangesAsync();

        return Ok(orderItem);
    }

    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(long key)
    {
        var orderItem = await ctx.OrderItem.FindAsync(key);

        if (orderItem != null)
        {
            ctx.OrderItem.Remove(orderItem);
            await ctx.SaveChangesAsync();
        }

        return NoContent();
    }
}
