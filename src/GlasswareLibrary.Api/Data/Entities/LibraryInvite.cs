namespace GlasswareLibrary.Api.Data.Entities;

public class LibraryInvite
{
    public Guid Id { get; set; }
    public Guid LibraryId { get; set; }
    public required byte[] TokenHash { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int MaxUses { get; set; } = 1;
    public int UseCount { get; set; }
    public DateTime? RevokedAt { get; set; }
}
