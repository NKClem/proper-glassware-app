namespace GlasswareLibrary.Api.Data.Entities;

/// <summary>Shared shape of BeerStyle and GlassStyle. Not mapped itself; each subclass has its own table.</summary>
public abstract class Style
{
    public int Id { get; set; }
    public Guid? LibraryId { get; set; }
    public required string Name { get; set; }
}
