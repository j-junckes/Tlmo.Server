using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Tlmo.Entities;
using Tlmo.Repository;

namespace Tlmo.Server.Extensions;

public static class HubExtension
{
  public static Guid GetUserId(this Hub hub)
  {
    var user = hub.Context.User;
    
    if (user == null)
    {
      throw new Exception("User context not found");
    }
    
    var sub = user.FindFirstValue(ClaimTypes.NameIdentifier);

    if (sub == null)
    {
      throw new Exception("User id not found");
    }

    return Guid.Parse(sub);
  }
  
  public static async Task<User> GetUserAsync(this Hub hub, TlmoContext context)
  {
    var userId = hub.GetUserId();
    
    var user = await context.Users.FirstOrDefaultAsync(u => u.Id.Equals(userId));

    if (user == null)
    {
      throw new Exception("User not found");
    }

    return user;
  }
  
  public static async Task<User> GetUserWithWorkspacesAsync(this Hub hub, TlmoContext context)
  {
    var userId = hub.GetUserId();

    var user = await context.Users
      .Include(u => u.Workspaces.Where(w => w.DeletedAt == null))
      .Include(u => u.OwnedWorkspaces.Where(w => w.DeletedAt == null))
      .FirstOrDefaultAsync(u => u.Id == userId);

    if (user == null)
    {
      throw new Exception("User not found");
    }

    if (user.DeletedAt != null)
    {
      throw new Exception("User is deleted");
    }

    return user;
  }
}
