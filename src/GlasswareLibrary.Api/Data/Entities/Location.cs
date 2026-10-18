namespace GlasswareLibrary.Api.Data.Entities;

public class Location
{
    public Guid Id { get; set; }
    public Guid LibraryId { get; set; }
    public required string Name { get; set; }
    public LocationType LocationType { get; set; }
    public string? PlaceId { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
