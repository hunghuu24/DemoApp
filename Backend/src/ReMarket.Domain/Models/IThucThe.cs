namespace ReMarket.Domain.Models;

public interface IThucThe
{
    Guid Id { get; set; }
    DateTimeOffset NgayTao { get; set; }
    byte[] PhienBan { get; set; }
}
