using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ReMarket.Domain.Enums;

namespace ReMarket.Application.SmartSwap;

/// <summary>Directed edges describe delivery: giver -> receiver. Only length-three simple cycles qualify.</summary>
public static partial class BoTimVongSmartSwap
{
    private sealed record Nut(UngVienSmartSwap Tin, HashSet<Guid> ToTien, HashSet<string> TuSanPham,
        List<HashSet<string>> TuMongMuon);
    private sealed record Canh(decimal Diem, List<string> LyDo);
    private sealed record Vong(int A, int B, int C, string Ma, decimal Diem, long TongBu);
    private static readonly HashSet<string> TuDem = new(
        "toi minh muon can nhan tim doi lay san pham mon mot chiec cai co the giup xin uu tien cho voi".Split(' '), StringComparer.Ordinal);
    [GeneratedRegex(@"[\p{L}\p{N}]+", RegexOptions.CultureInvariant)] private static partial Regex TuRegex();
    [GeneratedRegex(@"(?<=\p{L})(?=\p{N})|(?<=\p{N})(?=\p{L})", RegexOptions.CultureInvariant)] private static partial Regex TachSoRegex();

    public static string ChuanHoa(string value)
    {
        var result = new StringBuilder();
        foreach (var ch in value.Normalize(NormalizationForm.FormD).ToLowerInvariant())
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark) result.Append(ch == 'đ' ? 'd' : ch);
        return result.ToString().Normalize(NormalizationForm.FormC);
    }
    private static HashSet<string> Tu(string value) => TuRegex().Matches(TachSoRegex().Replace(ChuanHoa(value), " "))
        .Select(x => x.Value).ToHashSet(StringComparer.Ordinal);
    private static int Bac(TinhTrangSanPham value) => value switch
    { TinhTrangSanPham.NhuMoi => 3, TinhTrangSanPham.DaSuDungTot => 2, TinhTrangSanPham.CoLoiNhe => 1, _ => 0 };

    private static List<HashSet<string>> NhuCau(UngVienSmartSwap row)
    {
        if (row.TuKhoaMongMuon != null)
            return row.TuKhoaMongMuon.Split('|').Select(Tu).Where(x => x.Count > 0).ToList();
        if (row.DanhMucMongMuonId != null) return []; // Explicit category is authoritative; prose remains descriptive.
        var words = Tu(row.MoTaMongMuon);
        // Legacy prose has no formal negation/OR grammar. Do not turn negative or ambiguous wishes into matches.
        if (words.Overlaps(["khong", "ngoai", "tru", "hoac", "hay", "not", "except", "or"])) return [];
        words.ExceptWith(TuDem);
        return words.Count == 0 ? [] : [words];
    }
    private static Canh? Ghep(Nut giver, Nut receiver)
    {
        var p = giver.Tin; var need = receiver.Tin;
        if (p.NguoiDungId == need.NguoiDungId || p.SanPhamId == need.SanPhamId) return null;
        if (need.DanhMucMongMuonId is Guid category && !giver.ToTien.Contains(category)) return null;
        if (receiver.TuMongMuon.Count > 0)
        { if (!receiver.TuMongMuon.Any(words => words.IsSubsetOf(giver.TuSanPham))) return null; }
        else if (need.DanhMucMongMuonId == null || need.TuKhoaMongMuon != null) return null;
        if (Bac(p.TinhTrang) == 0 || (need.TinhTrangToiThieu is {} min && Bac(p.TinhTrang) < Bac(min))) return null;
        if (need.GiaTriMongMuonTu > p.GiaTriThamKhao || need.GiaTriMongMuonDen < p.GiaTriThamKhao) return null;
        var reasons = new List<string>();
        if (need.DanhMucMongMuonId != null) reasons.Add("Đúng danh mục muốn nhận (kể cả danh mục con).");
        if (receiver.TuMongMuon.Count > 0) reasons.Add(need.TuKhoaMongMuon == null ? "Đủ từ khóa trong nhu cầu cũ." : "Đủ từ khóa mong muốn.");
        reasons.Add("Đạt tình trạng tối thiểu và khoảng giá trị mong muốn.");
        var ratio = (decimal)Math.Min(p.GiaTriThamKhao, need.GiaTriThamKhao) / Math.Max(p.GiaTriThamKhao, need.GiaTriThamKhao);
        var quality = p.TinhTrang switch { TinhTrangSanPham.NhuMoi => 1m, TinhTrangSanPham.DaSuDungTot => .75m, _ => .4m };
        var sameCity = !string.IsNullOrWhiteSpace(p.TinhThanh) && ChuanHoa(p.TinhThanh.Trim()) == ChuanHoa(need.TinhThanh.Trim());
        if (sameCity) reasons.Add("Cùng tỉnh/thành phố.");
        return new(60m + 25m * ratio + 10m * quality + (sameCity ? 5m : 0m), reasons);
    }

    private static List<CanDoiSmartSwapDto>? CanDoi(IReadOnlyList<Nut> nodes, int[] cycle)
    {
        var members = new List<CanDoiSmartSwapDto>(3);
        for (var i = 0; i < 3; i++)
        {
            var own = nodes[cycle[i]].Tin;
            var received = nodes[cycle[(i + 2) % 3]].Tin;
            var net = received.GiaTriThamKhao - own.GiaTriThamKhao;
            // Net outward cash across ALL transfers, not a cap independently applied to each edge.
            if (net != 0 && !own.ChoPhepBuTien || net > own.SoTienBuToiDa) return null;
            members.Add(new(own.NguoiDungId, own.TinDangId, received.TinDangId, own.GiaTriThamKhao,
                received.GiaTriThamKhao, Math.Max(net, 0), Math.Max(-net, 0), own.SoTienBuToiDa));
        }
        return members;
    }
    private static List<KhoanBuSmartSwapDto> ChiaTien(List<CanDoiSmartSwapDto> members)
    {
        var result = new List<KhoanBuSmartSwapDto>();
        var payers = members.Where(x => x.TienCanBu > 0).OrderBy(x => x.NguoiDungId).ToArray();
        var payees = members.Where(x => x.TienDuocNhan > 0).OrderBy(x => x.NguoiDungId).ToArray();
        var remaining = payees.Select(x => x.TienDuocNhan).ToArray(); var j = 0;
        foreach (var payer in payers)
        {
            var debt = payer.TienCanBu;
            while (debt > 0)
            {
                var amount = Math.Min(debt, remaining[j]);
                result.Add(new(payer.NguoiDungId, payees[j].NguoiDungId, amount));
                debt -= amount; remaining[j] -= amount;
                if (remaining[j] == 0) j++;
            }
        }
        return result;
    }

    public static KetQuaTimSmartSwapDto Tim(IReadOnlyList<UngVienSmartSwap> listings, IReadOnlyList<DanhMucSmartSwap> categories,
        TimSmartSwapRequest filter, GioiHanTimSmartSwap limits, DateTimeOffset snapshot, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (filter.Trang is <1 or >1000 || filter.KichThuocTrang is <1 or >50 || filter.DiemToiThieu is <0 or >100)
            throw new ArgumentOutOfRangeException(nameof(filter));
        if (listings.Count > limits.SoTinToiDa) throw new VuotGioiHanSmartSwapException();
        var categoryMap = categories.ToDictionary(x => x.Id);
        var nodes = listings.OrderBy(x => x.TinDangId).Select(row =>
        {
            var ancestors = new HashSet<Guid>(); var names = new List<string> { row.TenSanPham };
            Guid? id = row.DanhMucId;
            while (id is Guid key && categoryMap.TryGetValue(key, out var category) && ancestors.Add(key))
            { names.Add(category.Ten); id = category.ChaId; }
            return new Nut(row, ancestors, Tu(string.Join(' ', names)), NhuCau(row));
        }).ToArray();
        if (nodes.Select(x => x.Tin.TinDangId).Distinct().Count() != nodes.Length) throw new ArgumentException("Tin đăng bị trùng.", nameof(listings));
        var graph = Enumerable.Range(0, nodes.Length).Select(_ => new Dictionary<int, Canh>()).ToArray();
        var edges = 0;
        for (var i = 0; i < nodes.Length; i++)
        {
            ct.ThrowIfCancellationRequested();
            if (nodes[i].Tin.GiaTriThamKhao is <=0 or >1_000_000_000_000) continue;
            for (var j = 0; j < nodes.Length; j++)
            {
                if (i == j || nodes[j].Tin.GiaTriThamKhao is <=0 or >1_000_000_000_000) continue;
                if (Ghep(nodes[i], nodes[j]) is {} edge)
                {
                    if (++edges > limits.SoCanhToiDa) throw new VuotGioiHanSmartSwapException();
                    graph[i].Add(j, edge);
                }
            }
        }
        var comparer = Comparer<Vong>.Create((x,y) =>
        {
            var score = y.Diem.CompareTo(x.Diem); if (score != 0) return score;
            var money = x.TongBu.CompareTo(y.TongBu); return money != 0 ? money : StringComparer.Ordinal.Compare(x.Ma,y.Ma);
        });
        var best = new SortedSet<Vong>(comparer);
        var keep = filter.Trang * filter.KichThuocTrang; var total = 0; var visited = 0;
        for (var a = 0; a < nodes.Length; a++)
        {
            ct.ThrowIfCancellationRequested();
            foreach (var (b, ab) in graph[a])
            {
                // a is the smallest listing index: rotations are the same cycle; reversal is a different delivery.
                if (b <= a) continue;
                foreach (var (c, bc) in graph[b])
                {
                    if (++visited > limits.SoLuotDuyetToiDa) throw new VuotGioiHanSmartSwapException();
                    if ((visited & 1023) == 0) ct.ThrowIfCancellationRequested();
                    if (c <= a || !graph[c].TryGetValue(a, out var ca)) continue;
                    int[] cycle = [a,b,c];
                    if (cycle.Select(x => nodes[x].Tin.NguoiDungId).Distinct().Count() != 3 ||
                        cycle.Select(x => nodes[x].Tin.SanPhamId).Distinct().Count() != 3) continue;
                    if (filter.TinDangId is Guid anchor && cycle.All(x => nodes[x].Tin.TinDangId != anchor)) continue;
                    var balance = CanDoi(nodes,cycle); if (balance == null) continue;
                    // Additional discovery filters narrow the selected listing's existing needs; they never change a listing.
                    if (filter.TinDangId is Guid selected)
                    {
                        var ownIndex = Array.FindIndex(cycle, x => nodes[x].Tin.TinDangId == selected);
                        var own = nodes[cycle[ownIndex]].Tin;
                        var received = nodes[cycle[(ownIndex+2)%3]];
                        if (filter.BuToiDa is long cap && balance.Single(x=>x.NguoiDungId==own.NguoiDungId).TienCanBu>cap) continue;
                        if (filter.TinhTrangToiThieu is {} min && Bac(received.Tin.TinhTrang)<Bac(min)) continue;
                        if (!string.IsNullOrWhiteSpace(filter.TuKhoaMuonNhan) && !Tu(filter.TuKhoaMuonNhan).IsSubsetOf(received.TuSanPham)) continue;
                    }
                    var score = Math.Round((ab.Diem + bc.Diem + ca.Diem) / 3m, 2, MidpointRounding.AwayFromZero);
                    if (score < filter.DiemToiThieu) continue;
                    total++;
                    var key = string.Join('.',cycle.Select(x => nodes[x].Tin.TinDangId.ToString("N")));
                    best.Add(new(a,b,c,key,score,balance.Sum(x => x.TienCanBu)));
                    if (best.Count > keep) best.Remove(best.Max!);
                }
            }
        }
        MonSmartSwapDto Product(UngVienSmartSwap row) => new(row.TinDangId,row.SanPhamId,row.NguoiDungId,
            row.TenSanPham,row.GiaTriThamKhao,row.TinhTrang.ToString(),row.TinhThanh,row.PhienBan);
        var page = best.Skip((filter.Trang-1)*filter.KichThuocTrang).Select(cycle =>
        {
            int[] indices = [cycle.A,cycle.B,cycle.C]; var legs = new List<ChangSmartSwapDto>(3);
            for (var i = 0; i < 3; i++)
            {
                var giver = nodes[indices[i]].Tin; var receiver = nodes[indices[(i+1)%3]].Tin; var edge = graph[indices[i]][indices[(i+1)%3]];
                legs.Add(new(giver.NguoiDungId,receiver.NguoiDungId,Product(giver),Math.Round(edge.Diem,2,MidpointRounding.AwayFromZero),edge.LyDo));
            }
            var members = CanDoi(nodes,indices)!;
            return new VongSmartSwapDeXuatDto(cycle.Ma,cycle.Diem,cycle.TongBu,legs,members,ChiaTien(members));
        }).ToList();
        return new(page,total,filter.Trang,filter.KichThuocTrang,(int)Math.Ceiling(total/(double)filter.KichThuocTrang),nodes.Length,edges,snapshot,
            total == 0 ? "Chưa tìm thấy vòng Smart Swap 3 bên phù hợp với nhu cầu và hạn mức tiền bù." :
            page.Count == 0 ? "Không có kết quả ở trang này." : "Các vòng được đề xuất từ tin đang khả dụng; chưa tạo giao dịch hoặc giữ sản phẩm.");
    }
}
