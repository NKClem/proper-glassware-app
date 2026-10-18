using System.ComponentModel.DataAnnotations;

namespace GlasswareLibrary.Api.Common;

public class AppOptions
{
    [Required]
    public required string PublicBaseUrl { get; init; }
}
