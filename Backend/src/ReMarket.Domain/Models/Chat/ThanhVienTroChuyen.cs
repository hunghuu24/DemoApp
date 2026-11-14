using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class ThanhVienTroChuyen : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid CuocTroChuyenId { get; set; }
    public Guid NguoiDungId { get; set; }
    public DateTimeOffset? LanDocCuoi { get; set; }
    public long SoThuTuDaDoc { get; set; }

    public CuocTroChuyen CuocTroChuyen { get; set; } = null!;
    public NguoiDung NguoiDung { get; set; } = null!;
}
