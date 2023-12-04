namespace Tlmo.Server.Hubs;

public interface IChatClient
{
  Task ReceiveMessage(Guid authorId, Guid workspaceId, Guid channelId, string message);
}