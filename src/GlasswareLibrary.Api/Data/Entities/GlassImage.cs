namespace GlasswareLibrary.Api.Data.Entities;

public class GlassImage
{
    public Guid Id { get; set; }
    public Guid GlassId { get; set; }
    public required string BlobName { get; set; }
    public required string ThumbBlobName { get; set; }
    public required string ContentType { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int SizeBytes { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
