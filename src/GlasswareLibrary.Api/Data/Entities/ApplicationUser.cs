using Microsoft.AspNetCore.Identity;

namespace GlasswareLibrary.Api.Data.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public required string DisplayName { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime? DisabledAt { get; set; }
}
