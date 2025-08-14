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
public class CustomerController(ApplicationDbContext ctx) : ControllerBase
{
    [HttpGet("")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<IQueryable<Customer>> Get()
    {
        return Ok(ctx.Customer);
    }

    [HttpGet("{key}")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Customer>> GetAsync(long key)
    {
        var customer = await ctx.Customer.FirstOrDefaultAsync(x => x.Id == key);

        if (customer == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(customer);
        }
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Customer>> PostAsync(Customer customer)
    {
        var record = await ctx.Customer.FindAsync(customer.Id);
        if (record != null)
        {
            return Conflict();
        }
    
        await ctx.Customer.AddAsync(customer);

        await ctx.SaveChangesAsync();

        return Created($"/customer/{customer.Id}", customer);
    }

    [HttpPut("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Customer>> PutAsync(long key, Customer update)
    {
        var customer = await ctx.Customer.FirstOrDefaultAsync(x => x.Id == key);

        if (customer == null)
        {
            return NotFound();
        }

        ctx.Entry(customer).CurrentValues.SetValues(update);

        await ctx.SaveChangesAsync();

        return Ok(customer);
    }

    [HttpPatch("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Customer>> PatchAsync(long key, Delta<Customer> delta)
    {
        var customer = await ctx.Customer.FirstOrDefaultAsync(x => x.Id == key);

        if (customer == null)
        {
            return NotFound();
        }

        delta.Patch(customer);

        await ctx.SaveChangesAsync();

        return Ok(customer);
    }

    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(long key)
    {
        var customer = await ctx.Customer.FindAsync(key);

        if (customer != null)
        {
            ctx.Customer.Remove(customer);
            await ctx.SaveChangesAsync();
        }

        return NoContent();
    }
}
