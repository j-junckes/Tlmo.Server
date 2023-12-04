using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace Tlmo.Server.Controllers.Workspace;

public record CreateWorkspaceDto
{
  [MinLength(3, ErrorMessage = "Slug should be at least 3 characters long")]
  [MaxLength(16, ErrorMessage = "Slug should be at maximum 16 characters long")]
  [RegularExpression("^[a-zA-Z0-9_]*$", ErrorMessage = "Slug can only contain letters, numbers and underscores")]
  [Required]
  public string Slug { get; set; } = null!;

  [MinLength(3, ErrorMessage = "Name should be at least 3 characters long")]
  [MaxLength(16, ErrorMessage = "Name should be at maximum 16 characters long")]
  [Required]
  public string Name { get; set; } = null!;
}

public record CreateWorkspaceResponseDto
{
  public Guid Id { get; set; }
  public string Slug { get; set; } = null!;
  public string Name { get; set; } = null!;
}

public record ListWorkspaceResponseDto
{
  public Guid Id { get; set; }
  public string Slug { get; set; } = null!;
  public string Name { get; set; } = null!;
}