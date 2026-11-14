namespace ReMarket.Infrastructure.Persistence;

// Kept alongside migration10 so SQL deployments and EF migrations install identical guards.
public static class SmartSwapDeliverySql
{
    public static readonly string[] Triggers = ["TR_SmartSwap_ThongTin", "TR_SmartSwap_VanChuyen", "TR_SmartSwap_Receipt", "TR_SmartSwap_HoanTat", "TR_SmartSwap_SuCo"];
    public static readonly string[] Create = [
        """
        CREATE OR ALTER TRIGGER dbo.TR_SmartSwap_ThongTin ON dbo.ThongTinGiaoNhanSmartSwap AFTER INSERT,UPDATE AS
        BEGIN
          SET NOCOUNT ON;
          IF EXISTS(SELECT 1 FROM inserted i WHERE NOT EXISTS(SELECT 1 FROM dbo.ThanhVienVong m JOIN dbo.VongSmartSwap v ON v.Id=m.VongSmartSwapId
            JOIN dbo.GiaoDich g ON g.Id=v.GiaoDichId JOIN dbo.PhienBanThoaThuan p ON p.Id=i.PhienBanThoaThuanId AND p.GiaoDichId=g.Id
            WHERE m.Id=i.ThanhVienVongId AND v.NguoiKhoiTaoId IS NOT NULL AND v.TrangThai IN ('SanSangBanGiao','DangBanGiao') AND g.TrangThai='DangThucHien' AND p.TrangThai='DaChot'))
            THROW 51030,N'Thong tin giao nhan sai vong/ban thoa thuan hoac chua du dong y.',1;
        END
        """,
        """
        CREATE OR ALTER TRIGGER dbo.TR_SmartSwap_VanChuyen ON dbo.VanChuyenMoPhong AFTER INSERT,UPDATE AS
        BEGIN
          SET NOCOUNT ON;
          IF EXISTS(SELECT 1 FROM inserted i WHERE i.ChangTraoDoiId IS NOT NULL AND NOT EXISTS(SELECT 1 FROM dbo.ChangTraoDoi c
            JOIN dbo.VongSmartSwap v ON v.Id=c.VongSmartSwapId JOIN dbo.GiaoDich g ON g.Id=v.GiaoDichId
            JOIN dbo.PhienBanThoaThuan p ON p.Id=i.PhienBanThoaThuanId AND p.GiaoDichId=g.Id
            WHERE c.Id=i.ChangTraoDoiId AND v.TrangThai IN ('SanSangBanGiao','DangBanGiao') AND g.TrangThai='DangThucHien' AND p.TrangThai='DaChot'))
            THROW 51031,N'Van chuyen Smart Swap chi hoat dong tren dung vong da chot va khong tam dung.',1;
          IF EXISTS(SELECT 1 FROM inserted i JOIN deleted d ON i.Id=d.Id WHERE d.ChangTraoDoiId IS NOT NULL AND
            (ISNULL(i.ChangTraoDoiId,'00000000-0000-0000-0000-000000000000')<>d.ChangTraoDoiId OR i.DonHangId IS NOT NULL OR i.PhienBanThoaThuanId<>d.PhienBanThoaThuanId
            OR i.TenNguoiGui<>d.TenNguoiGui OR i.DienThoaiGui<>d.DienThoaiGui OR i.DiaChiGui<>d.DiaChiGui
            OR i.TenNguoiNhan<>d.TenNguoiNhan OR i.DienThoaiNhan<>d.DienThoaiNhan OR i.DiaChiNhan<>d.DiaChiNhan
            OR (d.NgayNhanDungHang IS NOT NULL AND (i.NgayNhanDungHang IS NULL OR i.NgayNhanDungHang<>d.NgayNhanDungHang))))
            THROW 51031,N'Khong doi nguon/chang/dia chi hoac rut xac nhan da nhan.',1;
          IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.ChangTraoDoi c ON c.Id=i.ChangTraoDoiId JOIN dbo.VongSmartSwap v ON v.Id=c.VongSmartSwapId
            JOIN dbo.ThanhVienVong m ON m.Id=c.ThanhVienNhanId WHERE i.NgayNhanDungHang IS NOT NULL AND NOT EXISTS(SELECT 1 FROM dbo.XacNhanGiaoNhan x
            WHERE x.GiaoDichId=v.GiaoDichId AND x.PhienBanThoaThuanId=i.PhienBanThoaThuanId AND x.SanPhamId=c.SanPhamId AND x.NguoiXacNhanId=m.NguoiDungId AND x.Loai='DaNhan'))
            THROW 51031,N'Nhan dung hang can xac nhan cua dung nguoi nhan.',1;
        END
        """,
        """
        CREATE OR ALTER TRIGGER dbo.TR_SmartSwap_Receipt ON dbo.XacNhanGiaoNhan AFTER INSERT,UPDATE,DELETE AS
        BEGIN
          SET NOCOUNT ON;
          IF EXISTS(SELECT 1 FROM deleted d WHERE EXISTS(SELECT 1 FROM dbo.VongSmartSwap v JOIN dbo.ChangTraoDoi c ON c.VongSmartSwapId=v.Id
            JOIN dbo.VanChuyenMoPhong s ON s.ChangTraoDoiId=c.Id WHERE v.GiaoDichId=d.GiaoDichId))
            THROW 51032,N'Khong sua/xoa xac nhan giao nhan van don Smart Swap.',1;
          IF EXISTS(SELECT 1 FROM inserted i WHERE EXISTS(SELECT 1 FROM dbo.VongSmartSwap v JOIN dbo.ChangTraoDoi c ON c.VongSmartSwapId=v.Id
            JOIN dbo.VanChuyenMoPhong s ON s.ChangTraoDoiId=c.Id WHERE v.GiaoDichId=i.GiaoDichId)
            AND NOT EXISTS(SELECT 1 FROM dbo.VongSmartSwap v JOIN dbo.GiaoDich g ON g.Id=v.GiaoDichId JOIN dbo.ChangTraoDoi c ON c.VongSmartSwapId=v.Id
            JOIN dbo.VanChuyenMoPhong s ON s.ChangTraoDoiId=c.Id JOIN dbo.ThanhVienVong a ON a.Id=c.ThanhVienGiaoId JOIN dbo.ThanhVienVong b ON b.Id=c.ThanhVienNhanId
            WHERE v.GiaoDichId=i.GiaoDichId AND g.TrangThai='DangThucHien' AND v.TrangThai='DangBanGiao' AND s.PhienBanThoaThuanId=i.PhienBanThoaThuanId AND c.SanPhamId=i.SanPhamId
            AND ((i.Loai='DaGiao' AND i.NguoiXacNhanId=a.NguoiDungId AND i.NguoiDoiTacId=b.NguoiDungId AND s.TrangThai IN ('DaGui','DangVanChuyen','DaGiao'))
            OR (i.Loai='DaNhan' AND i.NguoiXacNhanId=b.NguoiDungId AND i.NguoiDoiTacId=a.NguoiDungId AND s.TrangThai='DaGiao'))
            AND NOT EXISTS(SELECT 1 FROM dbo.KhieuNai k WHERE k.GiaoDichId=g.Id AND k.VanChuyenMoPhongId IS NOT NULL AND k.TrangThai NOT IN ('DaGiaiQuyet','TuChoi'))))
            THROW 51032,N'Chua giao dung chang hoac vong dang co su co.',1;
        END
        """,
        """
        CREATE OR ALTER TRIGGER dbo.TR_SmartSwap_HoanTat ON dbo.VongSmartSwap AFTER INSERT,UPDATE AS
        BEGIN
          SET NOCOUNT ON;
          IF EXISTS(SELECT 1 FROM inserted i WHERE i.TrangThai='HoanTat' AND EXISTS(SELECT 1 FROM dbo.ChangTraoDoi c JOIN dbo.VanChuyenMoPhong s ON s.ChangTraoDoiId=c.Id WHERE c.VongSmartSwapId=i.Id)
            AND ((SELECT COUNT(*) FROM dbo.ChangTraoDoi c JOIN dbo.VanChuyenMoPhong s ON s.ChangTraoDoiId=c.Id WHERE c.VongSmartSwapId=i.Id AND s.NgayNhanDungHang IS NOT NULL AND s.TrangThai='DaGiao')<>3
            OR EXISTS(SELECT 1 FROM dbo.KhieuNai k WHERE k.GiaoDichId=i.GiaoDichId AND k.VanChuyenMoPhongId IS NOT NULL AND k.TrangThai NOT IN ('DaGiaiQuyet','TuChoi'))
            OR NOT EXISTS(SELECT 1 FROM dbo.GiaoDich g WHERE g.Id=i.GiaoDichId AND g.TrangThai IN ('DangThucHien','HoanTat'))))
            THROW 51033,N'Chi hoan tat khi du3 nguoi nhan dung hang va khong co su co.',1;
        END
        """,
        """
        CREATE OR ALTER TRIGGER dbo.TR_SmartSwap_SuCo ON dbo.KhieuNai AFTER INSERT,UPDATE AS
        BEGIN
          SET NOCOUNT ON;
          IF EXISTS(SELECT 1 FROM inserted i WHERE i.VanChuyenMoPhongId IS NOT NULL AND NOT EXISTS(SELECT 1 FROM dbo.VanChuyenMoPhong s
            JOIN dbo.ChangTraoDoi c ON c.Id=s.ChangTraoDoiId JOIN dbo.VongSmartSwap v ON v.Id=c.VongSmartSwapId
            JOIN dbo.ThanhVienVong a ON a.Id=c.ThanhVienGiaoId JOIN dbo.ThanhVienVong b ON b.Id=c.ThanhVienNhanId
            WHERE s.Id=i.VanChuyenMoPhongId AND v.GiaoDichId=i.GiaoDichId AND a.NguoiDungId=i.NguoiBiKhieuNaiId AND b.NguoiDungId=i.NguoiGuiId))
            THROW 51034,N'Su co phai thuoc dung van don va dung nguoi nhan.',1;
        END
        """
    ];
}
