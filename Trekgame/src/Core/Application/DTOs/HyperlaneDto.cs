namespace StarTrekGame.Application.DTOs;

public class HyperlaneDto
{
    public Guid Id { get; set; }
    public Guid FromSystemId { get; set; }
    public Guid ToSystemId { get; set; }
    public int Distance { get; set; }
}
