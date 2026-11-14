using System.ComponentModel.DataAnnotations;

namespace ReMarket.Application.SmartSwap;

public sealed record DanhGiaThanhVienSmartSwapRequest(Guid NguoiDuocDanhGiaId,[Range(1,5)]int SoSao,
    [Required,StringLength(2000,MinimumLength=5)]string NoiDung);
public sealed record DanhGiaSmartSwapRequest(Guid MaYeuCau,
    [Required,MinLength(2),MaxLength(2)]List<DanhGiaThanhVienSmartSwapRequest> DanhGia);
public sealed record DanhGiaSmartSwapDto(Guid Id,Guid NguoiDuocDanhGiaId,string HoTen,int SoSao,string NoiDung,DateTimeOffset NgayTao);
