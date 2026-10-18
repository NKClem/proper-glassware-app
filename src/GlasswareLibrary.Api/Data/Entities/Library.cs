namespace GlasswareLibrary.Api.Data.Entities;

public class Library
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? PublicDescription { get; set; }
    public DateTime? HiddenAt { get; set; }
    public Guid? HiddenByUserId { get; set; }
    public string? HiddenReason { get; set; }
    public DateTime CreatedAt { get; set; }
}
