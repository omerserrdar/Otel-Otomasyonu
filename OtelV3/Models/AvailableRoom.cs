namespace OtelV3.Models;

public class AvailableRoom
{
    public string DisplayName { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public decimal Price { get; set; }
    
    public override string ToString() => DisplayName;
}
