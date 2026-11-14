using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;
using ReMarket.Infrastructure.Persistence;

namespace ReMarket.Infrastructure.Seeding;

/// <summary>Local demo only. One atomic, repeatable seed; never called by normal API startup.</summary>
public static class DuLieuMau
{
    public static Guid Ma(string key) => new(SHA256.HashData(Encoding.UTF8.GetBytes("ReMarketDemo/v1/" + key))[..16]);
    private static readonly DateTimeOffset Moc = new(2026, 10, 29, 8, 0, 0, TimeSpan.Zero);
    private static string AnhDemo(string key) => key switch {
        "ban" => "/demo/figma-ban.jpg", "ke" => "/demo/figma-ke.jpg", "iphone" => "/demo/figma-iphone.png", _ => "/demo/san-pham.png"
    };

    public static async Task KhoiTaoAsync(ReMarketDbContext db, CancellationToken ct = default)
    {
        if ((await db.Database.GetPendingMigrationsAsync(ct)).Any())
            throw new InvalidOperationException("Hay ap dung migration truoc khi seed.");
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await db.Database.ExecuteSqlRawAsync("""
            DECLARE @ketQua int;
            EXEC @ketQua = sys.sp_getapplock @Resource=N'ReMarketDemo/v1', @LockMode='Exclusive',
                @LockOwner='Transaction', @LockTimeout=10000;
            IF @ketQua < 0 THROW 51000, N'Khong lay duoc khoa seed demo.', 1;
            """, ct);
        var markerId = Ma("seed-v1");
        if (await db.NhatKyQuanTri.AnyAsync(x => x.Id == markerId, ct))
        {
            // Upgrade only untouched fixture placeholders; never replace images uploaded by a user.
            foreach (var key in new[] { "ban", "ke", "iphone" })
            {
                var imageId = Ma("img-" + key); var path = AnhDemo(key);
                await db.AnhSanPham.Where(x => x.Id == imageId && x.DuongDan == "/demo/san-pham.png")
                    .ExecuteUpdateAsync(x => x.SetProperty(p => p.DuongDan, path), ct);
            }
            await transaction.CommitAsync(ct);
            return;
        }
        T Them<T>(T value, string key) where T : class, IThucThe
        {
            value.Id = Ma(key); value.NgayTao = Moc;
            db.Add(value); return value;
        }
        var userRole = await db.VaiTro.SingleOrDefaultAsync(x => x.Ma == "USER", ct)
            ?? Them(new VaiTro { Ma = "USER", Ten = "Người dùng" }, "role-user");
        var adminRole = await db.VaiTro.SingleOrDefaultAsync(x => x.Ma == "ADMIN", ct)
            ?? Them(new VaiTro { Ma = "ADMIN", Ten = "Quản trị viên" }, "role-admin");
        var password = Environment.GetEnvironmentVariable("REMARKET_DEMO_PASSWORD") ?? "ReMarketDemo!2026";
        var hasher = new PasswordHasher<NguoiDung>();
        var people = new (string Key, string Name)[] {
            ("linh", "Linh Nguyễn"), ("anh", "Nguyễn Minh Anh"), ("nam", "Nguyễn Hoàng Nam"),
            ("vy", "Thảo Vy"), ("bao", "Quốc Bảo"), ("huy", "Trần Gia Huy"), ("ha", "Lê Thu Hà")
        };
        for (var i = 0; i < people.Length; i++)
        {
            var (key, name) = people[i];
            var user = Them(new NguoiDung { Email = key + "@remarket.example.test", SoDienThoai = "090000000" + i,
                TrangThai = key == "bao" ? TrangThaiNguoiDung.DaKhoa : TrangThaiNguoiDung.HoatDong }, key);
            user.MatKhauBam = hasher.HashPassword(user, password);
            Them(new HoSoCaNhan { NguoiDungId = user.Id, HoTen = name, TinhThanh = "TP. Hồ Chí Minh",
                DiaChi = "Quận 1, TP. Hồ Chí Minh", DaXacMinhEmail = true, DaXacMinhDienThoai = true }, "profile-" + key);
            Them(new NguoiDungVaiTro { NguoiDungId = user.Id, VaiTroId = userRole.Id }, "role-user-" + key);
            if (key == "linh") Them(new NguoiDungVaiTro { NguoiDungId = user.Id, VaiTroId = adminRole.Id }, "role-admin-linh");
        }
        string[] cats = ["Điện tử", "Nội thất", "Thời trang", "Gia dụng", "Sách & học tập", "Thể thao"];
        for (var i = 0; i < cats.Length; i++) Them(new DanhMuc { Ma = "DM" + (i + 1), Ten = cats[i], ThuTu = i + 1, DangHienThi = true }, "cat" + i);
        Them(new DanhMuc { Ma = "MAY_DOC_SACH", Ten = "Máy đọc sách", DanhMucChaId = Ma("cat0"), ThuTu = 1, DangHienThi = true }, "cat-reader");
        await db.SaveChangesAsync(ct);

        var products = new (string Key, string Owner, string Name, long Price, int Cat, LoaiTinDang Type, TrangThaiTinDang State)[] {
            ("iphone", "vy", "iPhone 13 128GB", 7_200_000, 0, LoaiTinDang.MuaBan, TrangThaiTinDang.DaBan),
            ("sony", "anh", "Sony WH-CH720N", 1_200_000, 0, LoaiTinDang.TraoDoi, TrangThaiTinDang.DaBan),
            ("kindle", "nam", "Kindle Paperwhite 4", 1_600_000, 0, LoaiTinDang.TraoDoi, TrangThaiTinDang.DaBan),
            ("laptop", "anh", "Laptop Dell XPS 13", 8_000_000, 0, LoaiTinDang.TraoDoi, TrangThaiTinDang.DangHienThi),
            ("ipad", "nam", "iPad Air 4", 8_500_000, 0, LoaiTinDang.TraoDoi, TrangThaiTinDang.DangHienThi),
            ("canon", "vy", "Canon M50", 7_500_000, 0, LoaiTinDang.TraoDoi, TrangThaiTinDang.DangHienThi),
            ("ghe", "bao", "Ghế gỗ tự nhiên", 450_000, 1, LoaiTinDang.MuaBan, TrangThaiTinDang.ChoDuyet),
            ("ban", "vy", "Bàn trà gỗ nhỏ", 650_000, 1, LoaiTinDang.CaHai, TrangThaiTinDang.DangHienThi),
            ("ke", "anh", "Kệ sách 3 tầng", 320_000, 1, LoaiTinDang.MuaBan, TrangThaiTinDang.BanNhap),
            ("jbl", "huy", "Loa JBL Flip 5", 1_100_000, 0, LoaiTinDang.TraoDoi, TrangThaiTinDang.DaAn),
            ("logitech", "ha", "Bàn phím Logitech", 900_000, 0, LoaiTinDang.TraoDoi, TrangThaiTinDang.DaAn)
        };
        foreach (var p in products)
        {
            var product = Them(new SanPham { ChuSoHuuId = Ma(p.Owner), DanhMucId = Ma("cat" + p.Cat), Ten = p.Name,
                MoTa = "Dữ liệu demo: đã qua sử dụng, mô tả phục vụ kiểm tra cấu trúc.", GiaTriThamKhao = p.Price,
                TinhTrang = TinhTrangSanPham.DaSuDungTot }, p.Key);
            Them(new AnhSanPham { SanPhamId = product.Id, DuongDan = AnhDemo(p.Key), ThuTu = 0, LaAnhBia = true }, "img-" + p.Key);
            var listing = Them(new TinDang { SanPhamId = product.Id, NguoiDangId = Ma(p.Owner), LoaiTin = p.Type,
                GiaBan = p.Type == LoaiTinDang.TraoDoi ? null : p.Price, TrangThai = p.State,
                TinhThanh = "TP. Hồ Chí Minh", DiaDiem = "Quận 1", NgayDang = Moc,
                NguoiDuyetId = p.State is TrangThaiTinDang.ChoDuyet or TrangThaiTinDang.BanNhap ? null : Ma("linh") }, "tin-" + p.Key);
            if (p.Type != LoaiTinDang.MuaBan)
                Them(new NhuCauTraoDoi { TinDangId = listing.Id, MoTaMongMuon = "Món đồ điện tử hoạt động tốt, có thể thương lượng.",
                    ChoPhepBuTien = true, SoTienBuToiDa = 500_000, ChoPhepSmartSwap = true }, "need-" + p.Key);
        }
        await db.SaveChangesAsync(ct);

        GiaoDich GiaoDichMoi(string key, string code, LoaiGiaoDich type, params string[] members)
        {
            var gd = Them(new GiaoDich { Ma = code, Loai = type, TrangThai = TrangThaiGiaoDich.KhoiTao }, key);
            foreach (var member in members) Them(new ThanhVienGiaoDich { GiaoDichId = gd.Id, NguoiDungId = Ma(member), VaiTroHienThi = member }, key + "-member-" + member);
            return gd;
        }
        var purchase = GiaoDichMoi("purchase", "RM260018", LoaiGiaoDich.MuaBan, "anh", "vy");
        var exchange = GiaoDichMoi("exchange", "TD-1024", LoaiGiaoDich.TraoDoiMotMot, "anh", "nam");
        var swap = GiaoDichMoi("swap", "SS-0301", LoaiGiaoDich.SmartSwap, "anh", "nam", "vy");
        var disputeTrade = GiaoDichMoi("dispute-trade", "TD-1018", LoaiGiaoDich.TraoDoiMotMot, "huy", "ha");
        await db.SaveChangesAsync(ct);

        var order = Them(new DonHang { GiaoDichId = purchase.Id, NguoiMuaId = Ma("anh"), NguoiBanId = Ma("vy"),
            TenNguoiNhan = "Nguyễn Minh Anh", SoDienThoaiNhan = "0900000001", DiaChiNhan = "Quận 1, TP. Hồ Chí Minh",
            TienHang = 6_500_000, PhiVanChuyen = 30_000, TongTien = 6_530_000, TrangThai = TrangThaiDonHang.DangGiao }, "order");
        Them(new ChiTietDonHang { DonHangId = order.Id, TinDangId = Ma("tin-iphone"), SanPhamId = Ma("iphone"), TenSanPham = "iPhone 13 128GB", DonGia = 6_500_000, SoLuong = 1 }, "order-line");
        Them(new ThanhToan { DonHangId = order.Id, SoTien = 6_530_000, LaMoPhong = true, TrangThai = TrangThaiThanhToan.ThanhCong, MaThamChieu = "DEMO-PAY-001" }, "payment");
        var shipping = Them(new VanChuyenMoPhong { DonHangId = order.Id, MaVanDon = "RM-SIM-001", TrangThai = TrangThaiVanChuyen.DangVanChuyen, NgayGiaoDuKien = Moc.AddDays(3) }, "shipping");
        foreach (var (status, index) in new[] { TrangThaiVanChuyen.ChoLayHang, TrangThaiVanChuyen.DaLayHang, TrangThaiVanChuyen.DangVanChuyen }.Select((x, i) => (x, i)))
            Them(new LichSuVanChuyen { VanChuyenMoPhongId = shipping.Id, TrangThai = status, ThoiDiem = Moc.AddHours(index), MoTa = "Mốc vận chuyển mô phỏng " + status }, "shipping-event-" + index);
        Them(new DeNghiTraoDoi { GiaoDichId = exchange.Id, NguoiGuiId = Ma("anh"), NguoiNhanId = Ma("nam"), SanPhamGuiId = Ma("sony"), SanPhamNhanId = Ma("kindle"), TrangThai = TrangThaiDeNghi.DaChapNhan, NgayHetHan = Moc.AddDays(1) }, "offer");
        Them(new DeNghiTraoDoi { GiaoDichId = disputeTrade.Id, NguoiGuiId = Ma("ha"), NguoiNhanId = Ma("huy"), SanPhamGuiId = Ma("logitech"), SanPhamNhanId = Ma("jbl"), TrangThai = TrangThaiDeNghi.DaChapNhan, NgayHetHan = Moc.AddDays(1) }, "dispute-offer");
        Them(new PhienBanThoaThuan { GiaoDichId = exchange.Id, SoPhienBan = 1, NguoiTaoId = Ma("anh"), NoiDung = "Bù 300.000đ; đã được thay thế bởi phản đề nghị.", TrangThai = TrangThaiThoaThuan.DaThayThe }, "agreement-old");
        var agreement = Them(new PhienBanThoaThuan { GiaoDichId = exchange.Id, SoPhienBan = 2, NguoiTaoId = Ma("nam"), NoiDung = "Sony đổi Kindle, Minh Anh bù Nam 400.000đ trực tiếp.", DiaDiemHen = "Nhà văn hóa Thanh Niên", ThoiGianHen = Moc.AddDays(1), TrangThai = TrangThaiThoaThuan.DeXuat }, "agreement");
        var swapAgreement = Them(new PhienBanThoaThuan { GiaoDichId = swap.Id, SoPhienBan = 1, NguoiTaoId = Ma("anh"), NoiDung = "A giao laptop cho C, C giao Canon cho B, B giao iPad cho A.", DiaDiemHen = "Nhà văn hóa Thanh Niên", ThoiGianHen = Moc.AddDays(4), TrangThai = TrangThaiThoaThuan.DeXuat }, "swap-agreement");
        var disputeAgreement = Them(new PhienBanThoaThuan { GiaoDichId = disputeTrade.Id, SoPhienBan = 1, NguoiTaoId = Ma("ha"), NoiDung = "Loa JBL đổi bàn phím, Hà bù Huy 200.000đ.", TrangThai = TrangThaiThoaThuan.DeXuat }, "dispute-agreement");
        var cycle = Them(new VongSmartSwap { GiaoDichId = swap.Id, TrangThai = TrangThaiVong.ChoDongY, NgayHetHan = Moc.AddDays(2) }, "cycle");
        foreach (var (person, product, pos) in new[] { ("anh", "laptop", 1), ("nam", "ipad", 2), ("vy", "canon", 3) })
        {
            Them(new ThanhVienVong { VongSmartSwapId = cycle.Id, GiaoDichId = swap.Id, NguoiDungId = Ma(person), SanPhamId = Ma(product), ViTri = pos }, "cycle-" + person);
            Them(new GiuChoSanPham { SanPhamId = Ma(product), GiaoDichId = swap.Id, NgayHetHan = Moc.AddDays(7), DaGiaiPhong = false }, "reserve-" + product);
        }
        foreach (var (from, to, product) in new[] { ("anh", "vy", "laptop"), ("vy", "nam", "canon"), ("nam", "anh", "ipad") })
            Them(new ChangTraoDoi { VongSmartSwapId = cycle.Id, ThanhVienGiaoId = Ma("cycle-" + from), ThanhVienNhanId = Ma("cycle-" + to), SanPhamId = Ma(product) }, "leg-" + from);
        foreach (var (version, gd, users) in new[] { (agreement.Id, exchange.Id, new[] { "anh", "nam" }), (swapAgreement.Id, swap.Id, new[] { "anh", "nam", "vy" }), (disputeAgreement.Id, disputeTrade.Id, new[] { "huy", "ha" }) })
            foreach (var person in users) Them(new DongYThoaThuan { PhienBanThoaThuanId = version, GiaoDichId = gd, NguoiDungId = Ma(person), QuyetDinh = QuyetDinhDongY.DongY, NgayPhanHoi = Moc.AddHours(1) }, "consent-" + version + "-" + person);
        foreach (var (key, version, gd, payer, receiver, amount) in new[] {
            ("comp-old", Ma("agreement-old"), exchange.Id, "anh", "nam", 300_000L),
            ("comp", agreement.Id, exchange.Id, "anh", "nam", 400_000L),
            ("comp-swap-a", swapAgreement.Id, swap.Id, "anh", "nam", 500_000L),
            ("comp-swap-c", swapAgreement.Id, swap.Id, "vy", "nam", 500_000L),
            ("comp-dispute", disputeAgreement.Id, disputeTrade.Id, "ha", "huy", 200_000L) })
            Them(new TienBu { PhienBanThoaThuanId = version, GiaoDichId = gd, NguoiTraId = Ma(payer), NguoiNhanId = Ma(receiver), SoTien = amount,
                NguoiTraDaXacNhan = key != "comp-old", NguoiNhanDaXacNhan = key != "comp-old" }, key);
        await db.SaveChangesAsync(ct);
        agreement.TrangThai = swapAgreement.TrangThai = disputeAgreement.TrangThai = TrangThaiThoaThuan.DaChot;
        await db.SaveChangesAsync(ct);
        cycle.TrangThai = TrangThaiVong.DangBanGiao;
        await db.SaveChangesAsync(ct);
        purchase.TrangThai = swap.TrangThai = TrangThaiGiaoDich.DangThucHien;
        await db.SaveChangesAsync(ct);

        XacNhanGiaoNhan XacNhan(string key, Guid gd, string person, string other, string product, Guid version, LoaiXacNhan type) => Them(new XacNhanGiaoNhan {
            GiaoDichId = gd, NguoiXacNhanId = Ma(person), NguoiDoiTacId = Ma(other), SanPhamId = Ma(product),
            PhienBanThoaThuanId = version, Loai = type, NgayXacNhan = Moc.AddDays(1) }, key);
        var receipt = XacNhan("receipt-anh", exchange.Id, "anh", "nam", "kindle", agreement.Id, LoaiXacNhan.DaNhan);
        XacNhan("receipt-nam", exchange.Id, "nam", "anh", "sony", agreement.Id, LoaiXacNhan.DaNhan);
        foreach (var (person, other, product, type) in new[] {
            ("anh", "vy", "laptop", LoaiXacNhan.DaGiao),
            ("nam", "anh", "ipad", LoaiXacNhan.DaGiao), ("nam", "vy", "canon", LoaiXacNhan.DaNhan),
            ("vy", "nam", "canon", LoaiXacNhan.DaGiao), ("vy", "anh", "laptop", LoaiXacNhan.DaNhan) })
            XacNhan("swap-receipt-" + person + type, swap.Id, person, other, product, swapAgreement.Id, type);
        Them(new BangChungGiaoNhan { XacNhanGiaoNhanId = receipt.Id, NguoiTaiLenId = Ma("anh"), DuongDan = "/demo/san-pham.png", LoaiMime = "image/png" }, "receipt-evidence");
        await db.SaveChangesAsync(ct);
        exchange.TrangThai = TrangThaiGiaoDich.HoanTat; exchange.NgayHoanTat = Moc.AddDays(1);
        disputeTrade.TrangThai = TrangThaiGiaoDich.TranhChap;
        await db.SaveChangesAsync(ct);
        var review = Them(new DanhGia { GiaoDichId = exchange.Id, NguoiDanhGiaId = Ma("anh"), NguoiDuocDanhGiaId = Ma("nam"), SoSao = 5,
            NoiDung = "Nam đúng hẹn, Kindle đúng mô tả. Trao đổi rất thuận lợi!" }, "review");
        var criterion = await db.TieuChiDanhGia.SingleOrDefaultAsync(x=>x.Ma=="DUNG_MO_TA",ct)
            ?? Them(new TieuChiDanhGia { Ma = "DUNG_MO_TA", Ten = "Đúng mô tả" }, "criterion");
        Them(new DanhGiaTieuChi { DanhGiaId = review.Id, TieuChiDanhGiaId = criterion.Id }, "review-criterion");

        foreach (var (key, gd, members) in new[] { ("chat-purchase", purchase.Id, new[] { "anh", "vy" }), ("chat-exchange", exchange.Id, new[] { "anh", "nam" }), ("chat-swap", swap.Id, new[] { "anh", "nam", "vy" }) })
        {
            var chat = Them(new CuocTroChuyen { GiaoDichId = gd, Loai = members.Length == 3 ? LoaiTroChuyen.NhomSmartSwap : LoaiTroChuyen.TrucTiep, TieuDe = key == "chat-swap" ? "Nhóm SS-0301" : "Thương lượng ReMarket" }, key);
            foreach (var person in members) Them(new ThanhVienTroChuyen { CuocTroChuyenId = chat.Id, NguoiDungId = Ma(person), LanDocCuoi = Moc.AddMinutes(5) }, key + "-" + person);
            var message = Them(new TinNhan { CuocTroChuyenId = chat.Id, NguoiGuiId = Ma("anh"), MaPhiaClient = Ma(key + "-client"), Loai = LoaiTinNhan.VanBan, NoiDung = "Chào bạn, mình có thể kiểm tra sản phẩm khi gặp không?" }, key + "-message");
            if (key == "chat-swap") Them(new TepTinNhan { TinNhanId = message.Id, DuongDan = "/demo/san-pham.png", TenTep = "san-pham.png", LoaiMime = "image/png", DungLuong = 1000 }, "attachment");
        }
        Them(new ThongBao { NguoiDungId = Ma("anh"), GiaoDichId = swap.Id, Loai = LoaiThongBao.TraoDoi, TieuDe = "Đến lượt bạn xác nhận nhận iPad", NoiDung = "SS-0301 đã có 5/6 xác nhận giao nhận.", DaDoc = false }, "notification");
        var complaint = Them(new KhieuNai { Ma = "KN-031", GiaoDichId = disputeTrade.Id, NguoiGuiId = Ma("ha"), NguoiBiKhieuNaiId = Ma("huy"),
            TieuDe = "Loa JBL bị rè khi phát nhạc", NoiDung = "Loa rè trên 50% âm lượng, không đúng mô tả.", TrangThai = TrangThaiKhieuNai.ChoThucHien, HanPhanHoi = Moc.AddHours(24) }, "complaint");
        Them(new BangChungKhieuNai { KhieuNaiId = complaint.Id, NguoiTaiLenId = Ma("ha"), DuongDan = "/demo/san-pham.png", LoaiMime = "image/png", MoTa = "Ảnh demo, không phải bằng chứng giao dịch thật." }, "complaint-evidence");
        Them(new PhanHoiKhieuNai { KhieuNaiId = complaint.Id, NguoiGuiId = Ma("linh"), NoiDung = "Đã tiếp nhận thông tin và đối chiếu mô tả." }, "complaint-reply");
        Them(new QuyetDinhXuLy { KhieuNaiId = complaint.Id, QuanTriVienId = Ma("linh"), KetLuan = "Chấp nhận khiếu nại của Hà", BienPhap = "Hoàn trả hai sản phẩm và 200.000đ trực tiếp. Chờ xác minh thực hiện.", DaXacMinhThucHien = false }, "decision");
        var ticket = Them(new YeuCauHoTro { Ma = "HT-260018", NguoiGuiId = Ma("anh"), DonHangId = order.Id, Loai = LoaiYeuCauHoTro.DonHangVanChuyen,
            TieuDe = "Chậm cập nhật vận chuyển", NoiDung = "Nhờ kiểm tra thời gian giao dự kiến.", TrangThai = TrangThaiHoTro.DangXuLy }, "ticket");
        Them(new PhanHoiHoTro { YeuCauHoTroId = ticket.Id, NguoiGuiId = Ma("linh"), NoiDung = "Đang kiểm tra đơn vận chuyển mô phỏng." }, "ticket-reply");
        Them(new NhatKyKiemDuyet { TinDangId = Ma("tin-ban"), QuanTriVienId = Ma("linh"), TrangThaiTruoc = TrangThaiTinDang.ChoDuyet, TrangThaiSau = TrangThaiTinDang.DangHienThi, LyDo = "Thông tin và ảnh demo đủ điều kiện." }, "moderation");
        Them(new KhoaTaiKhoan { NguoiDungId = Ma("bao"), QuanTriVienId = Ma("linh"), LyDo = "Đăng tin trùng lặp nhiều lần (demo)", NgayBatDau = Moc, NgayKetThuc = Moc.AddDays(7) }, "restriction");
        Them(new NhatKyQuanTri { NguoiThucHienId = Ma("linh"), HanhDong = "KhoiTaoDuLieuMauV1", LoaiDoiTuong = "HeThong", ChiTietJson = "{\"phienBan\":1,\"laDuLieuDemo\":true}" }, "seed-v1");
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
}
