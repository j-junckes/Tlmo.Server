using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Tlmo.Entities;
using Tlmo.Repository;

namespace Tlmo.Server.Extensions;

public static class ControllerBaseExtension
{
  public static Guid GetUserId(this ControllerBase controller)
  {
    var sub = controller.User.FindFirstValue(ClaimTypes.NameIdentifier);

    if (sub == null)
    {
      throw new Exception("User id not found");
    }

    return Guid.Parse(sub);
  }

  public static async Task<User> GetUserAsync(this ControllerBase controller, TlmoContext context,
    CancellationToken cToken)
  {
    var userId = controller.GetUserId();

    var user = await context.Users
      .FirstOrDefaultAsync(u => u.Id == userId, cToken);

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
  
  public static async Task<User> GetUserWithWorkspacesAsync(this ControllerBase controller, TlmoContext context,
    CancellationToken cToken)
  {
    var userId = controller.GetUserId();

    var user = await context.Users
      .Include(u => u.Workspaces.Where(w => w.DeletedAt == null))
      .Include(u => u.OwnedWorkspaces.Where(w => w.DeletedAt == null))
      .FirstOrDefaultAsync(u => u.Id == userId, cToken);

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