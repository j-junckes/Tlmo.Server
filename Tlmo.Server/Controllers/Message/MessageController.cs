using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Tlmo.Repository;
using Tlmo.Server.Extensions;

namespace Tlmo.Server.Controllers.Message;

[ApiController, Route("workspaces/{workspaceId}/channels/{channelId}/messages")]
public class MessageController
  (ILogger<MessageController> logger, TlmoContext context, IConfiguration config) : ControllerBase
{
  // [HttpPost("create", Name = "CreateMessage"), Authorize]
  // public async Task<IActionResult> Create(Guid workspaceId, Guid channelId, [FromBody] CreateMessageDto model, CancellationToken cToken)
  // {
  //   await using var transaction = await context.Database.BeginTransactionAsync(cToken);
  //   try
  //   {
  //     var workspace = await context.Workspaces
  //       .Include(w => w.Users)
  //       .FirstOrDefaultAsync(w => w.Id == workspaceId, cToken);
  //
  //     if (workspace == null)
  //     {
  //       return BadRequest(new { messages = new[] { "error.workspace.not_found" } });
  //     }
  //
  //     var user = await this.GetUserAsync(context, cToken);
  //
  //     if (!workspace.Users.Any(u => u.Id.Equals(user.Id)))
  //     {
  //       return BadRequest(new { messages = new[] { "error.workspace.not_member" } });
  //     }
  //
  //     var channel = await context.Channels
  //       .Include(c => c.Workspace)
  //       .FirstOrDefaultAsync(c => c.Id == channelId, cToken);
  //
  //     if (channel == null)
  //     {
  //       return BadRequest(new { messages = new[] { "error.channel.not_found" } });
  //     }
  //
  //     if (!channel.WorkspaceId.Equals(workspace.Id))
  //     {
  //       return BadRequest(new { messages = new[] { "error.channel.not_in_workspace" } });
  //     }
  //
  //     var message = new Entities.Message
  //     {
  //       Content = model.Content,
  //       ChannelId = channel.Id,
  //       AuthorId = user.Id
  //     };
  //
  //     await context.Messages.AddAsync(message, cToken);
  //     await context.SaveChangesAsync(cToken);
  //
  //     await transaction.CommitAsync(cToken);
  //
  //     return Ok(new CreateMessageResponseDto
  //     {
  //       Id = message.Id,
  //       Content = message.Content,
  //       CreatedAt = message.CreatedAt.ToDateTimeUtc(),
  //       ChannelId = message.ChannelId,
  //       AuthorId = message.AuthorId
  //     });
  //   }
  //   catch (Exception e)
  //   {
  //     await transaction.RollbackAsync(cToken);
  //     logger.LogError(e, "Error creating message");
  //     return StatusCode(500, new { messages = new[] { "error.internal_server_error" } });
  //   }
  // }
  
  // [HttpGet("list", Name = "ListMessages"), Authorize]
  //
}