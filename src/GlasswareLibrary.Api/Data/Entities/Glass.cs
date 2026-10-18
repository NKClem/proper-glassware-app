namespace GlasswareLibrary.Api.Data.Entities;

public class Glass
{
    public Guid Id { get; set; }
    public Guid LibraryId { get; set; }
    public required string Name { get; set; }
    public string? Brewery { get; set; }
    public string? Description { get; set; }
    public int GlassStyleId { get; set; }
    public int BeerStyleId { get; set; }
    public Guid? LocationId { get; set; }
    public DateOnly? AcquiredDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<GlassImage> Images { get; set; } = [];
}
