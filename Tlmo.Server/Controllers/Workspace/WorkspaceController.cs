using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Tlmo.Repository;
using Tlmo.Server.Extensions;

namespace Tlmo.Server.Controllers.Workspace;

[ApiController, Route("workspaces")]
public class WorkspaceController
  (ILogger<WorkspaceController> logger, TlmoContext context, IConfiguration config) : ControllerBase
{
  [HttpPost("create", Name = "CreateWorkspace"), Authorize]
  public async Task<IActionResult> Create([FromBody] CreateWorkspaceDto model, CancellationToken cToken)
  {
    await using var transaction = await context.Database.BeginTransactionAsync(cToken);
    try
    {
      if (await context.Workspaces.AnyAsync(w => w.Slug == model.Slug, cToken))
      {
        return BadRequest(new { messages = new[] { "error.workspace.slug_taken" } });
      }

      var user = await this.GetUserAsync(context, cToken);

      var workspace = new Entities.Workspace
      {
        Slug = model.Slug,
        Name = model.Name,
        OwnerId = user.Id
      };

      await context.Workspaces.AddAsync(workspace, cToken);
      await context.SaveChangesAsync(cToken);

      workspace.Users.Add(user);

      await context.SaveChangesAsync(cToken);

      await transaction.CommitAsync(cToken);

      return Ok(new CreateWorkspaceResponseDto
      {
        Id = workspace.Id,
        Slug = workspace.Slug,
        Name = workspace.Name,
      });
    }
    catch (Exception e)
    {
      await transaction.RollbackAsync(cToken);
      logger.LogError(e, "Error while creating workspace");
      return BadRequest(new { messages = new[] { "error.workspace.create" } });
    }
  }

  [HttpGet("list", Name = "ListWorkspaces"), Authorize]
  public async Task<IActionResult> List(CancellationToken cToken)
  {
    try
    {
      var user = await this.GetUserWithWorkspacesAsync(context, cToken);

      var workspaces = user.Workspaces
        .Select(w => new ListWorkspaceResponseDto
        {
          Id = w.Id,
          Slug = w.Slug,
          Name = w.Name,
        }).ToList();

      return Ok(workspaces);
    }
    catch (Exception e)
    {
      logger.LogError(e, "Error while listing workspaces");
      return BadRequest(new { messages = new[] { "error.workspace.list" } });
    }
  }
}