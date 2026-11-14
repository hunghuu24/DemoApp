namespace ReMarket.Application.HeThong;

public sealed record TrangThaiHeThongDto(
    string TenUngDung,
    string PhienBan,
    bool CoSoDuLieuSanSang,
    DateTimeOffset ThoiGianUtc);
