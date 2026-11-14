using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReMarket.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ThemRangBuocNghiepVu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("EXEC(N'" + """
                CREATE OR ALTER TRIGGER [dbo].[TR_DonHang_LoaiGiaoDich] ON [dbo].[DonHang] AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM inserted i JOIN dbo.GiaoDich g ON g.Id=i.GiaoDichId WHERE g.Loai<>N'MuaBan')
                        THROW 51001, N'Loai giao dich khong phu hop voi bang chi tiet.', 1;
                END;
                """.Replace("'", "''") + "');");
            migrationBuilder.Sql("EXEC(N'" + """
                CREATE OR ALTER TRIGGER [dbo].[TR_DeNghiTraoDoi_LoaiGiaoDich] ON [dbo].[DeNghiTraoDoi] AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM inserted i JOIN dbo.GiaoDich g ON g.Id=i.GiaoDichId WHERE g.Loai<>N'TraoDoiMotMot')
                        THROW 51001, N'Loai giao dich khong phu hop voi bang chi tiet.', 1;
                END;
                """.Replace("'", "''") + "');");
            migrationBuilder.Sql("EXEC(N'" + """
                CREATE OR ALTER TRIGGER [dbo].[TR_VongSmartSwap_LoaiGiaoDich] ON [dbo].[VongSmartSwap] AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM inserted i JOIN dbo.GiaoDich g ON g.Id=i.GiaoDichId WHERE g.Loai<>N'SmartSwap')
                        THROW 51001, N'Loai giao dich khong phu hop voi bang chi tiet.', 1;
                END;
                """.Replace("'", "''") + "');");
            migrationBuilder.Sql("EXEC(N'" + """
                CREATE OR ALTER TRIGGER [dbo].[TR_ThanhVienGiaoDich_SoLuong] ON [dbo].[ThanhVienGiaoDich] AFTER INSERT, UPDATE, DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM dbo.GiaoDich g WHERE g.Id IN (SELECT GiaoDichId FROM inserted UNION SELECT GiaoDichId FROM deleted) AND (SELECT COUNT(*) FROM dbo.ThanhVienGiaoDich t WHERE t.GiaoDichId=g.Id)>CASE WHEN g.Loai=N'SmartSwap' THEN 3 ELSE 2 END)
                        THROW 51001, N'Vuot so thanh vien cua giao dich.', 1;
                    IF EXISTS (SELECT 1 FROM dbo.GiaoDich g WHERE g.Id IN (SELECT GiaoDichId FROM inserted UNION SELECT GiaoDichId FROM deleted) AND g.TrangThai NOT IN (N'KhoiTao',N'ChoDongY'))
                        THROW 51001, N'Khong sua thanh vien khi giao dich da bat dau.', 1;
                END;
                """.Replace("'", "''") + "');");
            migrationBuilder.Sql("EXEC(N'" + """
                CREATE OR ALTER TRIGGER [dbo].[TR_PhienBanThoaThuan_Chot] ON [dbo].[PhienBanThoaThuan] AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM inserted i JOIN deleted d ON d.Id=i.Id WHERE d.TrangThai=N'DaChot' AND (i.NoiDung<>d.NoiDung OR i.NguoiTaoId<>d.NguoiTaoId OR i.SoPhienBan<>d.SoPhienBan OR i.GiaoDichId<>d.GiaoDichId OR ISNULL(i.DiaDiemHen,N'')<>ISNULL(d.DiaDiemHen,N'') OR ISNULL(i.ThoiGianHen,'19000101')<>ISNULL(d.ThoiGianHen,'19000101') OR i.TrangThai=N'DeXuat'))
                        THROW 51001, N'Thoa thuan da chot phai duoc thay bang phien ban moi.', 1;
                    IF EXISTS (SELECT 1 FROM inserted i JOIN dbo.GiaoDich g ON g.Id=i.GiaoDichId WHERE i.TrangThai=N'DaChot' AND (g.Loai=N'MuaBan' OR (SELECT COUNT(*) FROM dbo.DongYThoaThuan d WHERE d.PhienBanThoaThuanId=i.Id AND d.QuyetDinh=N'DongY')<>CASE WHEN g.Loai=N'SmartSwap' THEN 3 ELSE 2 END))
                        THROW 51001, N'Chua du dong y cho phien ban thoa thuan.', 1;
                END;
                """.Replace("'", "''") + "');");
            migrationBuilder.Sql("EXEC(N'" + """
                CREATE OR ALTER TRIGGER [dbo].[TR_DongYThoaThuan_Khoa] ON [dbo].[DongYThoaThuan] AFTER INSERT, UPDATE, DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM dbo.PhienBanThoaThuan p WHERE p.Id IN (SELECT PhienBanThoaThuanId FROM inserted UNION SELECT PhienBanThoaThuanId FROM deleted) AND p.TrangThai=N'DaChot')
                        THROW 51001, N'Khong doi dong y tren thoa thuan da chot.', 1;
                END;
                """.Replace("'", "''") + "');");
            migrationBuilder.Sql("EXEC(N'" + """
                CREATE OR ALTER TRIGGER [dbo].[TR_TienBu_KhoaNoiDung] ON [dbo].[TienBu] AFTER INSERT, UPDATE, DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM inserted i FULL JOIN deleted d ON d.Id=i.Id LEFT JOIN dbo.PhienBanThoaThuan pi ON pi.Id=i.PhienBanThoaThuanId LEFT JOIN dbo.PhienBanThoaThuan pd ON pd.Id=d.PhienBanThoaThuanId WHERE (pi.TrangThai=N'DaChot' OR pd.TrangThai=N'DaChot') AND (i.Id IS NULL OR d.Id IS NULL OR i.SoTien<>d.SoTien OR i.NguoiTraId<>d.NguoiTraId OR i.NguoiNhanId<>d.NguoiNhanId OR i.PhienBanThoaThuanId<>d.PhienBanThoaThuanId OR i.GiaoDichId<>d.GiaoDichId))
                        THROW 51001, N'Khong doi noi dung tien bu da chot; tao phien ban moi.', 1;
                END;
                """.Replace("'", "''") + "');");
            migrationBuilder.Sql("EXEC(N'" + """
                CREATE OR ALTER TRIGGER [dbo].[TR_VongSmartSwap_DieuKien] ON [dbo].[VongSmartSwap] AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM inserted i WHERE i.TrangThai IN (N'SanSangBanGiao',N'DangBanGiao',N'HoanTat') AND ((SELECT COUNT(*) FROM dbo.ThanhVienVong t WHERE t.VongSmartSwapId=i.Id)<>3 OR (SELECT COUNT(*) FROM dbo.ChangTraoDoi c WHERE c.VongSmartSwapId=i.Id)<>3 OR NOT EXISTS (SELECT 1 FROM dbo.PhienBanThoaThuan p WHERE p.GiaoDichId=i.GiaoDichId AND p.TrangThai=N'DaChot' AND (SELECT COUNT(*) FROM dbo.DongYThoaThuan d WHERE d.PhienBanThoaThuanId=p.Id AND d.QuyetDinh=N'DongY')=3)))
                        THROW 51003, N'Vong can 3 thanh vien, 3 chang va 3/3 dong y.', 1;
                    IF EXISTS (SELECT 1 FROM inserted i WHERE i.TrangThai=N'HoanTat' AND (SELECT COUNT(*) FROM dbo.XacNhanGiaoNhan x JOIN dbo.PhienBanThoaThuan p ON p.Id=x.PhienBanThoaThuanId AND p.TrangThai=N'DaChot' WHERE x.GiaoDichId=i.GiaoDichId)<>6)
                        THROW 51006, N'Smart Swap chi hoan tat khi du 6/6 xac nhan.', 1;
                END;
                """.Replace("'", "''") + "');");
            migrationBuilder.Sql("EXEC(N'" + """
                CREATE OR ALTER TRIGGER [dbo].[TR_ThanhVienVong_Khoa] ON [dbo].[ThanhVienVong] AFTER INSERT, UPDATE, DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM dbo.VongSmartSwap v WHERE v.Id IN (SELECT VongSmartSwapId FROM inserted UNION SELECT VongSmartSwapId FROM deleted) AND v.TrangThai NOT IN (N'ChoDongY',N'DaHuy'))
                        THROW 51001, N'Khong doi thanh vien/chang khi vong da san sang ban giao.', 1;
                END;
                """.Replace("'", "''") + "');");
            migrationBuilder.Sql("EXEC(N'" + """
                CREATE OR ALTER TRIGGER [dbo].[TR_ChangTraoDoi_Khoa] ON [dbo].[ChangTraoDoi] AFTER INSERT, UPDATE, DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM dbo.VongSmartSwap v WHERE v.Id IN (SELECT VongSmartSwapId FROM inserted UNION SELECT VongSmartSwapId FROM deleted) AND v.TrangThai NOT IN (N'ChoDongY',N'DaHuy'))
                        THROW 51001, N'Khong doi thanh vien/chang khi vong da san sang ban giao.', 1;
                END;
                """.Replace("'", "''") + "');");
            migrationBuilder.Sql("EXEC(N'" + """
                CREATE OR ALTER TRIGGER [dbo].[TR_XacNhanGiaoNhan_HopLe] ON [dbo].[XacNhanGiaoNhan] AFTER INSERT, UPDATE, DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM dbo.GiaoDich g WHERE g.Id IN (SELECT GiaoDichId FROM inserted UNION SELECT GiaoDichId FROM deleted) AND g.TrangThai=N'HoanTat')
                        THROW 51001, N'Khong doi xac nhan cua giao dich da hoan tat.', 1;
                    IF EXISTS (SELECT 1 FROM inserted i JOIN dbo.GiaoDich g ON g.Id=i.GiaoDichId WHERE (g.Loai=N'MuaBan' AND NOT EXISTS (SELECT 1 FROM dbo.DonHang d JOIN dbo.ChiTietDonHang c ON c.DonHangId=d.Id WHERE d.GiaoDichId=i.GiaoDichId AND c.SanPhamId=i.SanPhamId AND i.PhienBanThoaThuanId IS NULL AND ((i.Loai=N'DaNhan' AND i.NguoiXacNhanId=d.NguoiMuaId AND i.NguoiDoiTacId=d.NguoiBanId) OR (i.Loai=N'DaGiao' AND i.NguoiXacNhanId=d.NguoiBanId AND i.NguoiDoiTacId=d.NguoiMuaId)))) OR (g.Loai=N'TraoDoiMotMot' AND NOT EXISTS (SELECT 1 FROM dbo.DeNghiTraoDoi d JOIN dbo.PhienBanThoaThuan p ON p.Id=i.PhienBanThoaThuanId AND p.GiaoDichId=d.GiaoDichId AND p.TrangThai=N'DaChot' WHERE d.GiaoDichId=i.GiaoDichId AND ((i.Loai=N'DaNhan' AND ((i.NguoiXacNhanId=d.NguoiGuiId AND i.NguoiDoiTacId=d.NguoiNhanId AND i.SanPhamId=d.SanPhamNhanId) OR (i.NguoiXacNhanId=d.NguoiNhanId AND i.NguoiDoiTacId=d.NguoiGuiId AND i.SanPhamId=d.SanPhamGuiId))) OR (i.Loai=N'DaGiao' AND ((i.NguoiXacNhanId=d.NguoiGuiId AND i.NguoiDoiTacId=d.NguoiNhanId AND i.SanPhamId=d.SanPhamGuiId) OR (i.NguoiXacNhanId=d.NguoiNhanId AND i.NguoiDoiTacId=d.NguoiGuiId AND i.SanPhamId=d.SanPhamNhanId)))))) OR (g.Loai=N'SmartSwap' AND NOT EXISTS (SELECT 1 FROM dbo.VongSmartSwap v JOIN dbo.ChangTraoDoi c ON c.VongSmartSwapId=v.Id JOIN dbo.ThanhVienVong a ON a.Id=c.ThanhVienGiaoId JOIN dbo.ThanhVienVong b ON b.Id=c.ThanhVienNhanId JOIN dbo.PhienBanThoaThuan p ON p.Id=i.PhienBanThoaThuanId AND p.GiaoDichId=v.GiaoDichId AND p.TrangThai=N'DaChot' WHERE v.GiaoDichId=i.GiaoDichId AND v.TrangThai IN (N'SanSangBanGiao',N'DangBanGiao') AND c.SanPhamId=i.SanPhamId AND ((i.Loai=N'DaGiao' AND i.NguoiXacNhanId=a.NguoiDungId AND i.NguoiDoiTacId=b.NguoiDungId) OR (i.Loai=N'DaNhan' AND i.NguoiXacNhanId=b.NguoiDungId AND i.NguoiDoiTacId=a.NguoiDungId)))))
                        THROW 51001, N'Xac nhan sai san pham, doi tac hoac phien ban thoa thuan.', 1;
                END;
                """.Replace("'", "''") + "');");
            migrationBuilder.Sql("EXEC(N'" + """
                CREATE OR ALTER TRIGGER [dbo].[TR_GiaoDich_HoanTat] ON [dbo].[GiaoDich] AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM inserted i WHERE (EXISTS(SELECT 1 FROM dbo.DonHang d WHERE d.GiaoDichId=i.Id) AND i.Loai<>N'MuaBan') OR (EXISTS(SELECT 1 FROM dbo.DeNghiTraoDoi d WHERE d.GiaoDichId=i.Id) AND i.Loai<>N'TraoDoiMotMot') OR (EXISTS(SELECT 1 FROM dbo.VongSmartSwap v WHERE v.GiaoDichId=i.Id) AND i.Loai<>N'SmartSwap'))
                        THROW 51001, N'Khong doi loai giao dich da co chi tiet.', 1;
                    IF EXISTS (SELECT 1 FROM inserted i WHERE i.TrangThai IN (N'DangThucHien',N'HoanTat') AND (SELECT COUNT(*) FROM dbo.ThanhVienGiaoDich t WHERE t.GiaoDichId=i.Id)<>CASE WHEN i.Loai=N'SmartSwap' THEN 3 ELSE 2 END)
                        THROW 51001, N'So thanh vien chua du de bat dau.', 1;
                    IF EXISTS (SELECT 1 FROM inserted i WHERE i.Loai=N'SmartSwap' AND i.TrangThai IN (N'DangThucHien',N'HoanTat') AND NOT EXISTS (SELECT 1 FROM dbo.VongSmartSwap v WHERE v.GiaoDichId=i.Id AND v.TrangThai IN (N'SanSangBanGiao',N'DangBanGiao',N'HoanTat') AND (i.TrangThai<>N'HoanTat' OR v.TrangThai=N'HoanTat')))
                        THROW 51001, N'Vong Smart Swap chua du dieu kien tuong ung.', 1;
                    IF EXISTS (SELECT 1 FROM inserted i WHERE i.Loai=N'TraoDoiMotMot' AND i.TrangThai IN (N'DangThucHien',N'HoanTat') AND (NOT EXISTS (SELECT 1 FROM dbo.DeNghiTraoDoi d WHERE d.GiaoDichId=i.Id) OR NOT EXISTS (SELECT 1 FROM dbo.PhienBanThoaThuan p WHERE p.GiaoDichId=i.Id AND p.TrangThai=N'DaChot') OR (i.TrangThai=N'HoanTat' AND (SELECT COUNT(*) FROM dbo.XacNhanGiaoNhan x JOIN dbo.PhienBanThoaThuan p ON p.Id=x.PhienBanThoaThuanId AND p.TrangThai=N'DaChot' WHERE x.GiaoDichId=i.Id AND x.Loai=N'DaNhan')<>2)))
                        THROW 51001, N'Trao doi 1-1 can thoa thuan va hai ben nhan du.', 1;
                    IF EXISTS (SELECT 1 FROM inserted i WHERE i.Loai=N'MuaBan' AND i.TrangThai IN (N'DangThucHien',N'HoanTat') AND NOT EXISTS (SELECT 1 FROM dbo.DonHang d WHERE d.GiaoDichId=i.Id AND d.TienHang=(SELECT SUM(c.DonGia*c.SoLuong) FROM dbo.ChiTietDonHang c WHERE c.DonHangId=d.Id) AND (i.TrangThai<>N'HoanTat' OR (d.TrangThai=N'DaNhan' AND EXISTS(SELECT 1 FROM dbo.XacNhanGiaoNhan x WHERE x.GiaoDichId=i.Id AND x.Loai=N'DaNhan')))))
                        THROW 51001, N'Don hang chua du chi tiet hoac chua nhan hang.', 1;
                END;
                """.Replace("'", "''") + "');");
            migrationBuilder.Sql("EXEC(N'" + """
                CREATE OR ALTER TRIGGER [dbo].[TR_DanhGia_DaHoanTat] ON [dbo].[DanhGia] AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM inserted i JOIN dbo.GiaoDich g ON g.Id=i.GiaoDichId WHERE g.TrangThai<>N'HoanTat')
                        THROW 51001, N'Chi danh gia giao dich da hoan tat.', 1;
                END;
                """.Replace("'", "''") + "');");
            migrationBuilder.Sql("EXEC(N'" + """
                CREATE OR ALTER TRIGGER [dbo].[TR_BangChungGiaoNhan_GioiHan] ON [dbo].[BangChungGiaoNhan] AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM inserted i WHERE i.LoaiMime NOT IN (N'image/jpeg',N'image/png'))
                        THROW 51001, N'Bang chung giao nhan chi nhan JPG/PNG.', 1;
                    IF EXISTS (SELECT 1 FROM dbo.BangChungGiaoNhan b WHERE b.XacNhanGiaoNhanId IN (SELECT XacNhanGiaoNhanId FROM inserted) GROUP BY b.XacNhanGiaoNhanId HAVING COUNT(*)>5)
                        THROW 51001, N'Toi da 5 anh bang chung cho mot xac nhan.', 1;
                END;
                """.Replace("'", "''") + "');");
            migrationBuilder.Sql("EXEC(N'" + """
                CREATE OR ALTER TRIGGER [dbo].[TR_AnhSanPham_GioiHan] ON [dbo].[AnhSanPham] AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM dbo.SanPham p WHERE p.Id IN (SELECT SanPhamId FROM inserted) AND (SELECT COUNT(*) FROM dbo.AnhSanPham a WHERE a.SanPhamId=p.Id)>CASE WHEN EXISTS(SELECT 1 FROM dbo.TinDang t WHERE t.SanPhamId=p.Id AND t.LoaiTin IN (N'TraoDoi',N'CaHai')) THEN 6 ELSE 8 END)
                        THROW 51001, N'Qua gioi han anh: 8 mua ban, 6 trao doi.', 1;
                END;
                """.Replace("'", "''") + "');");
            migrationBuilder.Sql("EXEC(N'" + """
                CREATE OR ALTER TRIGGER [dbo].[TR_TinDang_GioiHanAnh] ON [dbo].[TinDang] AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM dbo.SanPham p WHERE p.Id IN (SELECT SanPhamId FROM inserted) AND (SELECT COUNT(*) FROM dbo.AnhSanPham a WHERE a.SanPhamId=p.Id)>CASE WHEN EXISTS(SELECT 1 FROM dbo.TinDang t WHERE t.SanPhamId=p.Id AND t.LoaiTin IN (N'TraoDoi',N'CaHai')) THEN 6 ELSE 8 END)
                        THROW 51001, N'Qua gioi han anh theo loai tin.', 1;
                END;
                """.Replace("'", "''") + "');");
            migrationBuilder.Sql("EXEC(N'" + """
                CREATE OR ALTER TRIGGER [dbo].[TR_KhieuNai_DongPhieu] ON [dbo].[KhieuNai] AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM inserted i WHERE i.TrangThai=N'DaGiaiQuyet' AND NOT EXISTS(SELECT 1 FROM dbo.QuyetDinhXuLy q WHERE q.KhieuNaiId=i.Id AND q.DaXacMinhThucHien=1))
                        THROW 51001, N'Chi dong khieu nai sau khi xac minh thuc hien.', 1;
                END;
                """.Replace("'", "''") + "');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_KhieuNai_DongPhieu];");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_TinDang_GioiHanAnh];");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_AnhSanPham_GioiHan];");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_BangChungGiaoNhan_GioiHan];");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_DanhGia_DaHoanTat];");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_GiaoDich_HoanTat];");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_XacNhanGiaoNhan_HopLe];");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_ChangTraoDoi_Khoa];");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_ThanhVienVong_Khoa];");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_VongSmartSwap_DieuKien];");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_TienBu_KhoaNoiDung];");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_DongYThoaThuan_Khoa];");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_PhienBanThoaThuan_Chot];");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_ThanhVienGiaoDich_SoLuong];");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_VongSmartSwap_LoaiGiaoDich];");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_DeNghiTraoDoi_LoaiGiaoDich];");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_DonHang_LoaiGiaoDich];");
        }
    }
}
