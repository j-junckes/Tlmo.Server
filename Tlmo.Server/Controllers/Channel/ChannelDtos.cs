using System.ComponentModel.DataAnnotations;

namespace Tlmo.Server.Controllers.Channel;

public record CreateChannelDto
{
  [MinLength(3)]
  [MaxLength(16)]
  [Required]
  public string Slug { get; init; } = string.Empty;
}

public record CreateChannelResponseDto
{
  public Guid Id { get; init; }
  public string Slug { get; init; } = string.Empty;
}