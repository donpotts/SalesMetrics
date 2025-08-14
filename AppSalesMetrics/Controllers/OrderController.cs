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
public class OrderController(ApplicationDbContext ctx) : ControllerBase
{
    [HttpGet("")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<IQueryable<Order>> Get()
    {
        return Ok(ctx.Order.Include(x => x.Customer).Include(x => x.OrderItem));
    }

    [HttpGet("{key}")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Order>> GetAsync(long key)
    {
        var order = await ctx.Order.Include(x => x.Customer).Include(x => x.OrderItem).FirstOrDefaultAsync(x => x.Id == key);

        if (order == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(order);
        }
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Order>> PostAsync(Order order)
    {
        var record = await ctx.Order.FindAsync(order.Id);
        if (record != null)
        {
            return Conflict();
        }
    
        var orderItem = order.OrderItem;
        order.OrderItem = null;

        await ctx.Order.AddAsync(order);

        if (orderItem != null)
        {
            var newValues = await ctx.OrderItem.Where(x => orderItem.Select(y => y.Id).Contains(x.Id)).ToListAsync();
            order.OrderItem = [..newValues];
        }

        await ctx.SaveChangesAsync();

        return Created($"/order/{order.Id}", order);
    }

    [HttpPut("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Order>> PutAsync(long key, Order update)
    {
        var order = await ctx.Order.Include(x => x.OrderItem).FirstOrDefaultAsync(x => x.Id == key);

        if (order == null)
        {
            return NotFound();
        }

        ctx.Entry(order).CurrentValues.SetValues(update);

        if (update.OrderItem != null)
        {
            var updateValues = update.OrderItem.Select(x => x.Id);
            order.OrderItem ??= [];
            order.OrderItem.RemoveAll(x => !updateValues.Contains(x.Id));
            var addValues = updateValues.Where(x => !order.OrderItem.Select(y => y.Id).Contains(x));
            var newValues = await ctx.OrderItem.Where(x => addValues.Contains(x.Id)).ToListAsync();
            order.OrderItem.AddRange(newValues);
        }

        await ctx.SaveChangesAsync();

        return Ok(order);
    }

    [HttpPatch("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Order>> PatchAsync(long key, Delta<Order> delta)
    {
        var order = await ctx.Order.Include(x => x.OrderItem).FirstOrDefaultAsync(x => x.Id == key);

        if (order == null)
        {
            return NotFound();
        }

        delta.Patch(order);

        await ctx.SaveChangesAsync();

        return Ok(order);
    }

    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(long key)
    {
        var order = await ctx.Order.FindAsync(key);

        if (order != null)
        {
            ctx.Order.Remove(order);
            await ctx.SaveChangesAsync();
        }

        return NoContent();
    }
}
