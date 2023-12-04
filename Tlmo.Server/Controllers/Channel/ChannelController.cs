using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Tlmo.Repository;
using Tlmo.Server.Extensions;

namespace Tlmo.Server.Controllers.Channel;

[ApiController, Route("workspaces/{workspaceId}/channels")]
public class ChannelController
  (ILogger<ChannelController> logger, TlmoContext context, IConfiguration config) : ControllerBase
{
  [HttpPost("create", Name = "CreateChannel"), Authorize]
  public async Task<IActionResult> Create(Guid workspaceId, [FromBody] CreateChannelDto model, CancellationToken cToken)
  {
    await using var transaction = await context.Database.BeginTransactionAsync(cToken);
    try
    {
      var workspace = await context.Workspaces
        .Include(w => w.Users)
        .FirstOrDefaultAsync(w => w.Id == workspaceId, cToken);

      if (workspace == null)
      {
        return BadRequest(new { messages = new[] { "error.workspace.not_found" } });
      }

      var user = await this.GetUserAsync(context, cToken);

      if (!workspace.OwnerId.Equals(user.Id))
      {
        return BadRequest(new { messages = new[] { "error.workspace.not_owner" } });
      }

      if (await context.Channels.AnyAsync(c => c.Slug == model.Slug && c.WorkspaceId == workspace.Id, cToken))
      {
        return BadRequest(new { messages = new[] { "error.channel.slug_taken" } });
      }

      var channel = new Entities.Channel
      {
        Slug = model.Slug,
        WorkspaceId = workspace.Id
      };

      await context.Channels.AddAsync(channel, cToken);
      await context.SaveChangesAsync(cToken);

      await transaction.CommitAsync(cToken);

      return Ok(new CreateChannelResponseDto
      {
        Id = channel.Id,
        Slug = channel.Slug,
      });
    }
    catch (Exception e)
    {
      await transaction.RollbackAsync(cToken);
      logger.LogError(e, "Error while creating channel");
      return BadRequest(new { messages = new[] { "error.channel.create" } });
    }
  }
  
  [HttpGet("list", Name = "ListChannels"), Authorize]
  public async Task<IActionResult> List(Guid workspaceId, CancellationToken cToken)
  {
    try
    {
      var workspace = await context.Workspaces
        .Include(w => w.Users)
        .FirstOrDefaultAsync(w => w.Id == workspaceId, cToken);

      if (workspace == null)
      {
        return BadRequest(new { messages = new[] { "error.workspace.not_found" } });
      }

      var user = await this.GetUserAsync(context, cToken);

      if (!workspace.Users.Any(u => u.Id.Equals(user.Id)))
      {
        return BadRequest(new { messages = new[] { "error.workspace.not_member" } });
      }

      var channels = await context.Channels
        .Where(c => c.WorkspaceId == workspace.Id)
        .Select(c => new
        {
          c.Id,
          c.Slug
        })
        .ToListAsync(cToken);

      return Ok(channels);
    }
    catch (Exception e)
    {
      logger.LogError(e, "Error while listing channels");
      return BadRequest(new { messages = new[] { "error.channel.list" } });
    }
  }
}