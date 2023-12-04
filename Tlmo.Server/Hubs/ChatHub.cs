using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Tlmo.Repository;
using Tlmo.Server.Extensions;

namespace Tlmo.Server.Hubs;

public class ChatHub(ILogger<ChatHub> logger, TlmoContext context, IConfiguration config) : Hub<IChatClient>
{
  public async Task SendMessage(Guid workspaceId, Guid channelId, string message)
  {
    await using var transaction = await context.Database.BeginTransactionAsync();
    try
    {
      var workspace = await context.Workspaces
        .Include(w => w.Users)
        .FirstOrDefaultAsync(w => w.Id == workspaceId);

      if (workspace == null)
      {
        throw new Exception("Workspace not found");
      }

      var user = await this.GetUserAsync(context);

      if (!workspace.Users.Any(u => u.Id.Equals(user.Id)))
      {
        throw new Exception("User not member of workspace");
      }

      var channel = await context.Channels
        .Include(c => c.Workspace)
        .FirstOrDefaultAsync(c => c.Id == channelId);

      if (channel == null)
      {
        throw new Exception("Channel not found");
      }

      if (!channel.WorkspaceId.Equals(workspace.Id))
      {
        throw new Exception("Channel not in workspace");
      }

      var messageEntity = new Entities.Message
      {
        Content = message,
        ChannelId = channel.Id,
        AuthorId = user.Id
      };

      await context.Messages.AddAsync(messageEntity);
      await context.SaveChangesAsync();

      await transaction.CommitAsync();

      await Clients.Group(workspaceId.ToString()).ReceiveMessage(user.Id, workspace.Id, channelId, message);
    }
    catch (Exception e)
    {
      await transaction.RollbackAsync();
      logger.LogError(e, "Error sending message");
    }
  }

  public async Task JoinExistingWorkspaces()
  {
    try
    {
      var user = await this.GetUserWithWorkspacesAsync(context);

      foreach (var workspace in user.Workspaces)
      {
        await Groups.AddToGroupAsync(Context.ConnectionId, workspace.Id.ToString());
      }
    }
    catch (Exception e)
    {
      logger.LogError(e, "Error joining workspaces");
    }
  }

  public async Task JoinWorkspace(Guid workspaceId)
  {
    try
    {
      var user = await this.GetUserAsync(context);
      var workspace = await context.Workspaces
        .Include(w => w.Users)
        .FirstOrDefaultAsync(w => w.Id == workspaceId);

      if (workspace == null)
      {
        throw new Exception("Workspace not found");
      }

      if (!workspace.Users.Any(u => u.Id.Equals(user.Id)))
      {
        throw new Exception("User not member of workspace");
      }

      await Groups.AddToGroupAsync(Context.ConnectionId, workspaceId.ToString());
    }
    catch (Exception e)
    {
      logger.LogError(e, "Error joining workspace");
    }
  }

  public async Task LeaveWorkspace(Guid workspaceId)
  {
    try
    {
      await Groups.RemoveFromGroupAsync(Context.ConnectionId, workspaceId.ToString());
    }
    catch (Exception e)
    {
      logger.LogError(e, "Error leaving workspace");
    }
  }
}