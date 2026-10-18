namespace GlasswareLibrary.Api.Data.Entities;

public class LibraryMember
{
    public Guid LibraryId { get; set; }
    public Guid UserId { get; set; }
    public LibraryRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}
