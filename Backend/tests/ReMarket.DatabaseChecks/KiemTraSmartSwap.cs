using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReMarket.Api.Controllers;
using ReMarket.Application.SanPham;
using ReMarket.Application.SmartSwap;
using ReMarket.Application.TaiKhoan;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;
using ReMarket.Infrastructure.Persistence;
using SkiaSharp;

namespace ReMarket.DatabaseChecks;

internal static class KiemTraSmartSwap
{
    private static int passed;
    private static void Check(bool ok, string text)
    { if (!ok) throw new InvalidOperationException(text); passed++; Console.WriteLine("PASS " + text); }
    private static async Task Status(HttpResponseMessage response, int expected, string text)
    {
        if ((int)response.StatusCode != expected) throw new InvalidOperationException(text + ": " + (int)response.StatusCode + " " + await response.Content.ReadAsStringAsync());
        Check(true,text + " [" + expected + "]");
    }
    private static async Task<T> Post<T>(HttpClient client, string path, object body, int code = 200)
    {
        using var response = await client.PostAsJsonAsync(path,body); await Status(response,code,"POST " + path);
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    private static void DoThi()
    {
        var cat = new DanhMucSmartSwap(Guid.NewGuid(),null,"Đồ dùng");
        var now = new DateTimeOffset(2026,11,12,0,0,0,TimeSpan.Zero);
        var limits = new GioiHanTimSmartSwap();
        UngVienSmartSwap Row(string name, string wanted, long value = 1_000_000) => new(Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),
            name,cat.Id,value,TinhTrangSanPham.NhuMoi,"Hà Nội","version",wanted,null,null,null,null,null,true,5_000_000);
        UngVienSmartSwap[] Basic() => [Row("iPhone","Muốn nhận Laptop"),Row("Laptop","Muốn nhận iPad"),Row("iPad","Muốn nhận iPhone")];
        KetQuaTimSmartSwapDto Run(IReadOnlyList<UngVienSmartSwap> rows, TimSmartSwapRequest? filter = null,
            GioiHanTimSmartSwap? bound = null, IReadOnlyList<DanhMucSmartSwap>? cats = null) =>
            BoTimVongSmartSwap.Tim(rows,cats ?? [cat],filter ?? new(),bound ?? limits,now);
        var basic = Basic(); var result = Run(basic);
        Check(result.TongSo == 1 && result.Items.Single().Chang.Count == 3,"Exact directed triangle; rotations deduplicated");
        var ring = result.Items.Single();
        Check(ring.Chang.Any(x=>x.NguoiGiaoId==basic[0].NguoiDungId&&x.NguoiNhanId==basic[2].NguoiDungId) &&
              ring.Chang.Any(x=>x.NguoiGiaoId==basic[2].NguoiDungId&&x.NguoiNhanId==basic[1].NguoiDungId) &&
              ring.Chang.Any(x=>x.NguoiGiaoId==basic[1].NguoiDungId&&x.NguoiNhanId==basic[0].NguoiDungId),"Delivery orientation A -> C -> B -> A");
        Check(ring.DiemPhuHop == 100 && ring.TienBu.Count == 0,"Equal-value same-city new items score100 without cash");
        Check(Run(basic.Reverse().ToArray()).Items.Single().MaDeXuat == ring.MaDeXuat,"Input order does not change cycle identity");
        Check(Run([]).Items.Count == 0 && Run([]).ThongBao.Length > 0,"Empty graph returns empty items and message");
        Check(Run(basic.Take(2).ToArray()).TongSo == 0,"Two nodes are not a three-person cycle");
        Check(Run([Row("Violin","Muốn nhận Violin")]).TongSo == 0,"Self loop is excluded");
        Check(Run([Row("Violin","Muốn nhận Telescope"),Row("Telescope","Muốn nhận Violin")]).TongSo == 0,"Reciprocal 1-to-1 edge is not a triangle");
        Check(Run([Row("Violin","Muốn nhận Telescope"),Row("Telescope","Muốn nhận Book"),Row("Book","Muốn nhận Lamp"),Row("Lamp","Muốn nhận Violin")]).TongSo == 0,"Four-person cycle is excluded");
        Check(Run(basic.Select((x,i)=>i==2?x with{NguoiDungId=basic[0].NguoiDungId}:x).ToArray()).TongSo == 0,"Three products belonging to only two people are excluded");
        Check(Run(basic.Select((x,i)=>i==2?x with{SanPhamId=basic[0].SanPhamId}:x).ToArray()).TongSo == 0,"Repeated product cannot appear in a cycle");
        Check(Run(basic.Select((x,i)=>i==2?x with{MoTaMongMuon="Muốn nhận Telescope"}:x).ToArray()).TongSo == 0,"A missing directed demand edge breaks the cycle");
        Check(Run(basic.Append(Row("Isolated","Muốn nhận Missing")).ToArray()).TongSo == 1,"Unrelated nodes do not hide valid cycle");

        var accent = new[]{Row("Máy đọc sách15","Tôi muốn nhận TỦ GỖ7"),Row("Tủ gỗ7","Muốn nhận ĐÈN BÀN3"),Row("Đèn bàn3","Muốn nhận may DOC sach 15")};
        Check(Run(accent).TongSo == 1,"Vietnamese case, accents, d-stroke and letter-digit boundaries normalized");
        Check(Run(basic.Select((x,i)=>i==2?x with{MoTaMongMuon="Muốn nhận iPhones"}:x).ToArray()).TongSo == 0,"Whole tokens avoid accidental substring matching");
        Check(Run(basic.Select(x=>x with{MoTaMongMuon="Tôi muốn nhận một sản phẩm"}).ToArray()).TongSo == 0,"Generic empty wishes do not match every item");
        Check(Run(basic.Select((x,i)=>i==2?x with{MoTaMongMuon="Không muốn nhận iPhone"}:x).ToArray()).TongSo == 0,"Ambiguous legacy negation is not treated as positive demand");
        Check(Run(basic.Select((x,i)=>i==2?x with{TuKhoaMongMuon="Violin|iphone"}:x).ToArray()).TongSo == 1,"Explicit positive keyword alternatives are supported");
        Check(Run(basic.Select((x,i)=>i==2?x with{TuKhoaMongMuon="iphone missing"}:x).ToArray()).TongSo == 0,"All tokens within a keyword alternative are required");
        Check(Run(basic.Select((x,i)=>i==2?x with{TuKhoaMongMuon="|||"}:x).ToArray()).TongSo == 0,"Empty explicit keyword expression is not a wildcard");
        var parent = new DanhMucSmartSwap(Guid.NewGuid(),null,"Danh mục cha");
        var child = cat with{ChaId=parent.Id};
        var broad = basic.Select(x=>x with{DanhMucMongMuonId=parent.Id}).ToArray();
        Check(Run(broad,cats:[parent,child]).TongSo == 2,"Parent category accepts descendants; reverse delivery remains a distinct cycle");
        Check(Run(broad.Select((x,i)=>i==0?x with{DanhMucMongMuonId=Guid.NewGuid()}:x).ToArray(),cats:[parent,child]).TongSo == 0,"Unrelated target category is rejected");
        var worn = basic.Select((x,i)=>i==0?x with{TinhTrang=TinhTrangSanPham.CoLoiNhe}:i==2?x with{TinhTrangToiThieu=TinhTrangSanPham.DaSuDungTot}:x).ToArray();
        Check(Run(worn).TongSo == 0,"Receiver minimum condition is enforced on delivered product");
        worn[0] = worn[0] with{TinhTrang=TinhTrangSanPham.DaSuDungTot};
        Check(Run(worn).TongSo == 1,"Minimum condition boundary is inclusive");
        Check(Run(basic.Select((x,i)=>i==2?x with{GiaTriMongMuonTu=1_000_000,GiaTriMongMuonDen=1_000_000}:x).ToArray()).TongSo == 1,"Exact wanted value range is inclusive");
        Check(Run(basic.Select((x,i)=>i==2?x with{GiaTriMongMuonTu=1_000_001}:x).ToArray()).TongSo == 0,"Below wanted value minimum is excluded");
        Check(Run(basic.Select((x,i)=>i==2?x with{GiaTriMongMuonDen=999_999}:x).ToArray()).TongSo == 0,"Above wanted value maximum is excluded");

        var money = Basic(); money[0] = money[0] with{GiaTriThamKhao=10_000_000,SoTienBuToiDa=2_000_000};
        money[1] = money[1] with{GiaTriThamKhao=12_000_000,SoTienBuToiDa=0};
        money[2] = money[2] with{GiaTriThamKhao=8_000_000,SoTienBuToiDa=2_000_000};
        var paid = Run(money).Items.Single();
        Check(paid.TongTienBu == 4_000_000 && paid.TienBu.Count == 2,"Two net payers compensate the net recipient directly");
        Check(paid.ThanhVien.All(x=>x.GiaTriCho+x.TienCanBu==x.GiaTriNhan+x.TienDuocNhan),"Each participant conserves value after compensation");
        Check(paid.TienBu.Sum(x=>x.SoTien)==paid.ThanhVien.Sum(x=>x.TienDuocNhan),"Money transfers are balanced without external payer");
        Check(paid.ThanhVien.All(m=>paid.TienBu.Where(x=>x.NguoiTraId==m.NguoiDungId).Sum(x=>x.SoTien)==m.TienCanBu &&
            paid.TienBu.Where(x=>x.NguoiNhanId==m.NguoiDungId).Sum(x=>x.SoTien)==m.TienDuocNhan),"Transfer allocation matches every person's net balance");
        Check(Run(money.Select((x,i)=>i==0?x with{SoTienBuToiDa=1_999_999}:x).ToArray()).TongSo == 0,"One VND above payer cap excludes cycle");
        Check(Run(money.Select((x,i)=>i==1?x with{ChoPhepBuTien=false}:x).ToArray()).TongSo == 0,"Money recipient must also opt into compensation");
        Check(Run(basic.Select(x=>x with{ChoPhepBuTien=false,SoTienBuToiDa=0}).ToArray()).TongSo == 1,"No-compensation listings can exchange equal values");
        var split = money.Select((x,i)=>x with{GiaTriThamKhao=i==0?12_000_000:i==1?10_000_000:8_000_000,SoTienBuToiDa=i==2?3_000_000:0}).ToArray();
        Check(Run(split).TongSo == 0,"Cap is total outward cash, not a separate cap for each recipient");
        split[2] = split[2] with{SoTienBuToiDa=4_000_000};
        Check(Run(split).Items.Single().TienBu.Count == 2,"One net payer can settle two recipients within aggregate cap");
        Check(Run(money.Select(x=>x with{GiaTriThamKhao=1_000_000_000_000,ChoPhepBuTien=false,SoTienBuToiDa=0}).ToArray()).TongSo == 1,"Maximum supported prices do not overflow");
        Check(Run(basic.Select((x,i)=>i==0?x with{GiaTriThamKhao=0}:x).ToArray()).TongSo == 0,"Invalid zero-value product is not matched");

        var lower = new[]{Row("Violin","Muốn nhận Telescope"),Row("Telescope","Muốn nhận Book"),Row("Book","Muốn nhận Violin")}.Select(x=>x with{TinhTrang=TinhTrangSanPham.DaSuDungTot}).ToArray();
        var both = basic.Concat(lower).ToArray(); var sorted = Run(both);
        Check(sorted.TongSo==2 && sorted.Items[0].DiemPhuHop>sorted.Items[1].DiemPhuHop,"Results ranked by transparent suitability score");
        Check(Run(both,new(){DiemToiThieu=99}).TongSo==1,"Minimum suitability filters before counting and paging");
        var page1=Run(both,new(){KichThuocTrang=1}); var page2=Run(both,new(){Trang=2,KichThuocTrang=1});
        Check(page1.TongSo==2 && page1.TongTrang==2 && page1.Items[0].MaDeXuat==sorted.Items[0].MaDeXuat && page2.Items[0].MaDeXuat==sorted.Items[1].MaDeXuat,"Ranking precedes pagination and pages do not duplicate cycles");
        Check(Run(both,new(){TinDangId=lower[0].TinDangId}).TongSo==1,"Optional anchor matches any position in canonical cycle");
        Check(Run(both,new(){Trang=3,KichThuocTrang=1}).Items.Count==0,"Beyond last page returns an empty page");
        void Bound(Action action, string message)
        { var rejected=false;try{action();}catch(VuotGioiHanSmartSwapException){rejected=true;}Check(rejected,message); }
        Bound(()=>Run(both,bound:new(){SoTinToiDa=3}),"Candidate overflow is explicit, not silently truncated");
        Bound(()=>Run(broad,bound:new(){SoCanhToiDa=3},cats:[parent,child]),"Graph edge budget returns an explicit limit error");
        Bound(()=>Run(broad,bound:new(){SoLuotDuyetToiDa=1},cats:[parent,child]),"Traversal budget never returns partial successful results");
        using(var cancelled=new CancellationTokenSource())
        { cancelled.Cancel();var rejected=false;try{BoTimVongSmartSwap.Tim([],[],new(),limits,now,cancelled.Token);}catch(OperationCanceledException){rejected=true;}Check(rejected,"Search observes request cancellation even on empty data"); }
        var dense=Enumerable.Range(0,60).Select(_=>Row("Object "+Guid.NewGuid().ToString("N"),"") with{DanhMucMongMuonId=cat.Id,ChoPhepBuTien=false,SoTienBuToiDa=0}).ToArray();
        var denseResult=Run(dense,new(){KichThuocTrang=1});
        Check(denseResult.TongSo==60*59*58/3&&denseResult.Items.Count==1,"Dense60-node graph exhaustively counts68440 directed cycles while retaining only requested top page");

        // Independent brute-force oracle on generated graphs, unrelated to the phone/laptop example.
        var random=new Random(20261112);var categories=Enumerable.Range(0,3).Select(i=>new DanhMucSmartSwap(Guid.NewGuid(),null,"Category"+i)).ToArray();
        for(var run=0;run<100;run++)
        {
            var owners=Enumerable.Range(0,6).Select(_=>Guid.NewGuid()).ToArray();var products=Enumerable.Range(0,8).Select(_=>Guid.NewGuid()).ToArray();
            var rows=Enumerable.Range(0,random.Next(3,10)).Select(_=>Row(Guid.NewGuid().ToString("N"),"") with{
                NguoiDungId=owners[random.Next(owners.Length)],SanPhamId=products[random.Next(products.Length)],
                DanhMucId=categories[random.Next(3)].Id,DanhMucMongMuonId=categories[random.Next(3)].Id,ChoPhepBuTien=false,SoTienBuToiDa=0}).ToArray();
            var expected=new HashSet<string>(StringComparer.Ordinal);
            for(var a=0;a<rows.Length;a++)for(var b=0;b<rows.Length;b++)for(var c=0;c<rows.Length;c++)
            {
                var triple=new[]{rows[a],rows[b],rows[c]};
                if(triple.Select(x=>x.NguoiDungId).Distinct().Count()!=3||triple.Select(x=>x.SanPhamId).Distinct().Count()!=3)continue;
                if(rows[b].DanhMucMongMuonId!=rows[a].DanhMucId||rows[c].DanhMucMongMuonId!=rows[b].DanhMucId||rows[a].DanhMucMongMuonId!=rows[c].DanhMucId)continue;
                var min=Array.IndexOf(triple,triple.MinBy(x=>x.TinDangId)!);
                expected.Add(string.Join('.',Enumerable.Range(0,3).Select(i=>triple[(min+i)%3].TinDangId.ToString("N"))));
            }
            var actual=Run(rows,new(){KichThuocTrang=50},cats:categories);
            // These small graphs have at most 2*C(9,3)=168 cycles; collect subsequent pages as needed.
            var keys=actual.Items.Select(x=>x.MaDeXuat).ToHashSet(StringComparer.Ordinal);
            for(var page=2;page<=actual.TongTrang;page++)keys.UnionWith(Run(rows,new(){Trang=page,KichThuocTrang=50},cats:categories).Items.Select(x=>x.MaDeXuat));
            Check(expected.SetEquals(keys)&&actual.TongSo==expected.Count,"Generated directed graph agrees with independent oracle #"+run);
        }
    }

    private sealed class Factory(string connection, string folder, string signingKey, int maxCandidates=1500) : WebApplicationFactory<SmartSwapsController>
    {
        protected override void ConfigureWebHost(IWebHostBuilder b) => b.UseContentRoot(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..","..","..","..","..","src","ReMarket.Api")))
            .UseEnvironment("Development").ObserveApi().ConfigureLogging(x=>x.ClearProviders()).ConfigureAppConfiguration((_,c)=>c.AddInMemoryCollection(new Dictionary<string,string?>{
                ["ConnectionStrings:ReMarket"]=connection,["Jwt:SigningKey"]=signingKey,["Media:ProductFolder"]=folder,["RateLimits:PermitLimit"]="10000",
                ["Commerce:EnableExpirationWorker"]="false",["Exchange:EnableExpirationWorker"]="false",["SmartSwapSearch:SoTinToiDa"]=maxCandidates.ToString()}));
    }

    public static async Task<int> ChayAsync()
    {
        passed=0;
        try { DoThi(); await HttpSqlAsync(); Console.WriteLine($"Smart Swap graph/HTTP/SQL checks: {passed} passed. Generated graphs and live SQL; no credentials printed."); return 0; }
        catch(Exception ex) { Console.Error.WriteLine("FAIL Smart Swap: "+ex); return 1; }
    }

    private static async Task HttpSqlAsync()
    {
        var name="ReMarketTest_"+Guid.NewGuid().ToString("N");
        var tempRoot=Path.GetFullPath(Path.GetTempPath());var folder=Path.GetFullPath(Path.Combine(tempRoot,"remarket-smart-swap-"+name));
        var cs=new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("ConnectionStrings__ReMarket")??"Server=localhost;Trusted_Connection=True;TrustServerCertificate=True"){InitialCatalog="master"};
        await using var master=new SqlConnection(cs.ConnectionString);await master.OpenAsync();var created=false;Factory? factory=null;
        try
        {
            await new SqlCommand($"CREATE DATABASE [{name}]",master).ExecuteNonQueryAsync();created=true;cs.InitialCatalog=name;
            var signingKey=Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));factory=new Factory(cs.ConnectionString,folder,signingKey);
            using var a=factory.CreateClient();using var b=factory.CreateClient();using var c=factory.CreateClient();using var admin=factory.CreateClient();using var anonymous=factory.CreateClient();
            async Task<T> Db<T>(Func<ReMarketDbContext,Task<T>> fn)
            { await using var scope=factory.Services.CreateAsyncScope();return await fn(scope.ServiceProvider.GetRequiredService<ReMarketDbContext>()); }
            async Task Change(Func<ReMarketDbContext,Task> fn) => await Db(async db=>{await fn(db);await db.SaveChangesAsync();return true;});
            await Db(async db=>{await db.Database.MigrateAsync();Check(!db.Database.HasPendingModelChanges(),"SQL model matches migration8");return true;});
            async Task<Guid> Account(HttpClient client,string label,string phone)
            {
                var email=label+name+"@example.test";
                var profile=await Post<HoSoDto>(client,"/api/taikhoan/dangky",new DangKyRequest(label,email,phone,"SmartSwapTest!2026","SmartSwapTest!2026",true),201);
                var login=await Post<DangNhapDto>(client,"/api/taikhoan/dangnhap",new DangNhapRequest(email,"SmartSwapTest!2026"));
                client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",login.AccessToken);return profile.Id;
            }
            var aid=await Account(a,"SmartA","0911111111");var bid=await Account(b,"SmartB","0922222222");var cid=await Account(c,"SmartC","0933333333");var adminId=await Account(admin,"SmartModerator","0944444444");
            var root=new DanhMuc{Ten="Điện tử",Ma="smart-root",DangHienThi=true};
            var phone=new DanhMuc{Ten="Điện thoại",Ma="smart-phone",DanhMucChaId=root.Id,DangHienThi=true};
            var laptop=new DanhMuc{Ten="Laptop",Ma="smart-laptop",DanhMucChaId=root.Id,DangHienThi=true};
            var tablet=new DanhMuc{Ten="Máy tính bảng",Ma="smart-tablet",DanhMucChaId=root.Id,DangHienThi=true};
            await Change(async db=>{db.AddRange(root,phone,laptop,tablet);db.Add(new NguoiDungVaiTro{NguoiDungId=adminId,VaiTroId=await db.VaiTro.Where(x=>x.Ma=="ADMIN").Select(x=>x.Id).SingleAsync()});});
            byte[] picture;using(var bitmap=new SKBitmap(100,80)){bitmap.Erase(SKColors.Green);using var data=bitmap.Encode(SKEncodedImageFormat.Png,100);picture=data.ToArray();}
            LuuTinDangRequest Form(string title,Guid category,long price,string want,Guid wantedCategory,long cap,TieuChiSmartSwapRequest? criteria=null) =>
                new(title,category,price,"NhuMoi","Sản phẩm còn hoạt động tốt, đủ phụ kiện để kiểm tra.","Hà Nội","Địa điểm demo riêng tư",null,
                    new("Muốn nhận "+want,false,true,true,cap,null,criteria??new(wantedCategory,want,"DaSuDungTot",0,20_000_000)));
            var formA=Form("iPhone 13 demo",phone.Id,10_000_000,"Laptop",laptop.Id,2_000_000);
            var formB=Form("Laptop ThinkPad demo",laptop.Id,12_000_000,"iPad",tablet.Id,0);
            var formC=Form("iPad Air demo",tablet.Id,8_000_000,"iPhone",phone.Id,2_000_000);
            async Task<TinDangDto> Publish(HttpClient owner,LuuTinDangRequest form)
            {
                var tin=await Post<TinDangDto>(owner,"/api/tindang",form,201);
                using var upload=new MultipartFormDataContent();upload.Add(new StringContent(tin.PhienBan),"phienBan");
                var bytes=new ByteArrayContent(picture);bytes.Headers.ContentType=new("image/png");upload.Add(bytes,"anh","demo.png");
                using var image=await owner.PostAsync($"/api/tindang/{tin.Id}/anh",upload);await Status(image,200,"Upload real Smart Swap image");tin=(await image.Content.ReadFromJsonAsync<TinDangDto>())!;
                tin=await Post<TinDangDto>(owner,$"/api/tindang/{tin.Id}/gui-duyet",new PhienBanTinRequest(tin.PhienBan));
                return await Post<TinDangDto>(admin,$"/api/admin/tindang/{tin.Id}/duyet",new DuyetTinRequest(tin.PhienBan,true,null));
            }
            var ta=await Publish(a,formA);var tb=await Publish(b,formB);var tc=await Publish(c,formC);
            Check(ta.TraoDoi?.TieuChiSmartSwap?.DanhMucMongMuonId==laptop.Id&&ta.TraoDoi.TieuChiSmartSwap.GiaTriMongMuonDen==20_000_000,"Structured wishes persist through real create/upload/review API");
            await Status(await anonymous.GetAsync("/api/smart-swaps/search"),401,"Anonymous cannot use search");
            foreach(var query in new[]{"trang=0","kichThuocTrang=51","diemToiThieu=101","diemToiThieu=100.01","diemToiThieu=-0.01","tinDangId=not-a-guid","tinhThanh="+new string('x',101)})
                await Status(await a.GetAsync("/api/smart-swaps/search?"+query),400,"Invalid search query");
            async Task<KetQuaTimSmartSwapDto> Search(string query="")
            {
                using var response=await a.GetAsync("/api/smart-swaps/search"+query);await Status(response,200,"Search active SQL graph");
                Check(response.Headers.CacheControl?.NoStore==true,"Search response is not cached as inventory");
                return (await response.Content.ReadFromJsonAsync<KetQuaTimSmartSwapDto>())!;
            }
            async Task<string> Counts() => await Db(async db=>string.Join(',',new[]{
                await db.GiaoDich.CountAsync(),await db.VongSmartSwap.CountAsync(),await db.ThanhVienVong.CountAsync(),await db.ChangTraoDoi.CountAsync(),
                await db.TienBu.CountAsync(),await db.GiuChoSanPham.CountAsync(),await db.ThongBao.CountAsync(),await db.DeNghiTraoDoi.CountAsync()}));
            var before=await Counts();var first=await Search();
            Check(first.TongSo==1&&first.SoTinKhaDung==3,"Find cycle from SQL with no pre-registered circle");
            Check(first.Items[0].Chang.Any(x=>x.NguoiGiaoId==aid&&x.NguoiNhanId==cid)&&first.Items[0].Chang.Any(x=>x.NguoiGiaoId==cid&&x.NguoiNhanId==bid)&&first.Items[0].Chang.Any(x=>x.NguoiGiaoId==bid&&x.NguoiNhanId==aid),"REST returns delivery A -> C -> B -> A");
            Check(first.Items[0].TongTienBu==4_000_000&&first.Items[0].ThanhVien.Single(x=>x.NguoiDungId==bid).TienDuocNhan==4_000_000,"REST calculates per-person net compensation from SQL values");
            Check(first.Items[0].Chang.All(x=>x.SanPham.PhienBan.Length>0)&&first.Items[0].ThanhVien.Select(x=>x.NguoiDungId).Distinct().Count()==3,"Each returned circle includes three distinct owners and listing versions");
            Check(before==await Counts(),"Search creates no transaction, circle, member, leg, money, hold, notification or offer");
            Check((await a.GetFromJsonAsync<TinDangDto>($"/api/tindang/{ta.Id}"))!.PhienBan==ta.PhienBan,"Search does not modify listing rowversion");
            Check((await Search("?tinDangId="+tc.Id)).TongSo==1,"Anchor works for a noncanonical cycle position");
            var beyond=await Search("?trang=2&kichThuocTrang=1");Check(beyond.Items.Count==0&&beyond.TongSo==1,"REST pagination retains total count on empty page");
            var noCity=await Search("?tinhThanh=Da%20Nang");Check(noCity.Items.Count==0&&noCity.TongSo==0&&noCity.ThongBao.Length>0,"No matching location returns200 empty result and message");
            Check((await Search("?tinhThanh=ha%20noi")).TongSo==1,"City filter is accent-insensitive in SQL");
            await Status(await a.GetAsync("/api/smart-swaps/search?tinDangId="+Guid.NewGuid()),404,"Unavailable anchor has no private listing data");

            async Task Need(Guid id,Action<NhuCauTraoDoi> edit) => await Change(async db=>edit(await db.NhuCauTraoDoi.SingleAsync(x=>x.TinDangId==id)));
            async Task Tin(Guid id,Action<TinDang> edit) => await Change(async db=>edit(await db.TinDang.SingleAsync(x=>x.Id==id)));
            async Task Product(Guid id,Action<SanPham> edit) => await Change(async db=>edit(await db.SanPham.SingleAsync(x=>x.Id==id)));
            async Task Empty(string message) { var found=await Search();Check(found.Items.Count==0&&found.TongSo==0&&found.ThongBao.Length>0,message); }
            // Exercise SQL eligibility independently, restoring exactly the fixture changed each time.
            await Need(tc.Id,x=>{x.ChoPhepSmartSwap=false;x.ChoPhepMotMot=true;});await Empty("Only Smart-Swap-enabled products are considered");
            await Need(tc.Id,x=>{x.ChoPhepSmartSwap=true;x.ChoPhepMotMot=false;});
            foreach(var state in Enum.GetValues<TrangThaiTinDang>().Where(x=>x!=TrangThaiTinDang.DangHienThi))
            { await Tin(tc.Id,x=>x.TrangThai=state);await Empty("Inactive listing excluded: "+state); }
            await Tin(tc.Id,x=>x.TrangThai=TrangThaiTinDang.DangHienThi);
            await Tin(tc.Id,x=>x.NgayXoa=DateTimeOffset.UtcNow);await Empty("Soft-deleted listing excluded");await Tin(tc.Id,x=>x.NgayXoa=null);
            await Tin(tc.Id,x=>{x.LoaiTin=LoaiTinDang.MuaBan;x.GiaBan=8_000_000;});await Empty("Sale-only listing excluded even if old need exists");
            await Tin(tc.Id,x=>x.LoaiTin=LoaiTinDang.CaHai);Check((await Search()).TongSo==1,"Mixed sale/exchange listing is eligible when Smart Swap enabled");await Tin(tc.Id,x=>{x.LoaiTin=LoaiTinDang.TraoDoi;x.GiaBan=null;});
            await Change(async db=>(await db.NguoiDung.SingleAsync(x=>x.Id==cid)).TrangThai=TrangThaiNguoiDung.DaKhoa);await Empty("Locked owner excluded from candidate SQL");
            await Change(async db=>(await db.NguoiDung.SingleAsync(x=>x.Id==cid)).TrangThai=TrangThaiNguoiDung.HoatDong);
            await Change(async db=>(await db.DanhMuc.SingleAsync(x=>x.Id==root.Id)).DangHienThi=false);await Empty("Hidden ancestor hides descendant product and wanted categories");
            await Change(async db=>(await db.DanhMuc.SingleAsync(x=>x.Id==root.Id)).DangHienThi=true);
            await Need(tc.Id,x=>x.DanhMucMongMuonId=laptop.Id);await Empty("Unmet desired category excluded");await Need(tc.Id,x=>x.DanhMucMongMuonId=phone.Id);
            await Need(tc.Id,x=>x.TuKhoaMongMuon="UnrelatedProduct");await Empty("Unmet keyword demand excluded");await Need(tc.Id,x=>x.TuKhoaMongMuon="iPhone");
            await Product(ta.SanPhamId,x=>x.TinhTrang=TinhTrangSanPham.CoLoiNhe);await Empty("Delivered condition worse than wanted minimum excluded");await Product(ta.SanPhamId,x=>x.TinhTrang=TinhTrangSanPham.NhuMoi);
            await Need(ta.Id,x=>x.GiaTriMongMuonDen=11_999_999);await Empty("Received product above wanted range excluded");await Need(ta.Id,x=>{x.GiaTriMongMuonTu=12_000_000;x.GiaTriMongMuonDen=12_000_000;});
            Check((await Search()).TongSo==1,"Wanted exact value boundary works in SQL projection");await Need(ta.Id,x=>{x.GiaTriMongMuonTu=0;x.GiaTriMongMuonDen=20_000_000;});
            await Need(tc.Id,x=>x.SoTienBuToiDa=1_999_999);await Empty("SQL payer cap enforced");await Need(tc.Id,x=>x.SoTienBuToiDa=2_000_000);
            await Need(tb.Id,x=>x.ChoPhepBuTien=false);await Empty("Receiver compensation opt-in enforced");await Need(tb.Id,x=>x.ChoPhepBuTien=true);
            var gd=new GiaoDich{Ma="HOLD"+Guid.NewGuid().ToString("N")[..20],Loai=LoaiGiaoDich.SmartSwap,TrangThai=TrangThaiGiaoDich.ChoDongY};
            var hold=new GiuChoSanPham{GiaoDichId=gd.Id,SanPhamId=tc.SanPhamId,NgayTao=DateTimeOffset.UtcNow.AddDays(-2),NgayHetHan=DateTimeOffset.UtcNow.AddDays(-1)};
            await Change(_=>{_.Add(gd);_.Add(hold);return Task.CompletedTask;});await Empty("Unreleased hold excludes item even when metadata expiry passed");
            await Change(async db=>(await db.GiuChoSanPham.SingleAsync(x=>x.Id==hold.Id)).DaGiaiPhong=true);Check((await Search()).TongSo==1,"Released hold makes item discoverable again");

            var alternate=await Publish(b,formB with{Ten="Laptop ThinkPad second demo"});
            var two=await Search();Check(two.TongSo==2&&two.Items.All(x=>x.ThanhVien.Select(m=>m.NguoiDungId).Distinct().Count()==3),"Multiple items per owner yield alternative circles with one item per owner each");
            var p1=await Search("?kichThuocTrang=1");var p2=await Search("?kichThuocTrang=1&trang=2");
            Check(p1.TongSo==2&&p1.Items[0].MaDeXuat!=p2.Items[0].MaDeXuat,"HTTP stable ranked pagination over SQL circles");
            using(var limitedFactory=new Factory(cs.ConnectionString,folder,signingKey,3))
            using(var limited=limitedFactory.CreateClient())
            {
                limited.DefaultRequestHeaders.Authorization=a.DefaultRequestHeaders.Authorization;
                await Status(await limited.GetAsync("/api/smart-swaps/search"),422,"Candidate limit cannot silently hide circles");
            }

            // Backward-compatible edit: omission preserves new criteria, explicit {} clears them.
            var own=(await b.GetFromJsonAsync<TinDangDto>($"/api/tindang/{alternate.Id}/quanly"))!;
            var legacy=formB with{Ten=own.Ten,PhienBan=own.PhienBan,TraoDoi=formB.TraoDoi! with{TieuChiSmartSwap=null}};
            using var saved=await b.PutAsJsonAsync($"/api/tindang/{own.Id}",legacy);await Status(saved,200,"Existing client can edit without new fields");
            var updated=(await saved.Content.ReadFromJsonAsync<TinDangDto>())!;Check(updated.TraoDoi!.TieuChiSmartSwap!.DanhMucMongMuonId==tablet.Id,"Omitted criteria preserved during old-client edit");
            using var cleared=await b.PutAsJsonAsync($"/api/tindang/{own.Id}",legacy with{PhienBan=updated.PhienBan,TraoDoi=legacy.TraoDoi! with{TieuChiSmartSwap=new()}});
            await Status(cleared,200,"Explicit empty criteria clears optional constraints");var clearDto=(await cleared.Content.ReadFromJsonAsync<TinDangDto>())!;
            Check(clearDto.TraoDoi!.TieuChiSmartSwap!.DanhMucMongMuonId==null,"Explicit clear persisted");
            foreach(var criteria in new[]{new TieuChiSmartSwapRequest(GiaTriMongMuonTu:20,GiaTriMongMuonDen:10),new(TinhTrangToiThieu:"InvalidCondition"),new(DanhMucMongMuonId:Guid.NewGuid()),new(GiaTriMongMuonDen:-1)})
                await Status(await a.PostAsJsonAsync("/api/tindang",formA with{TraoDoi=formA.TraoDoi! with{TieuChiSmartSwap=criteria}}),400,"Invalid structured wish rejected");
            await Db(async db=>
            {
                foreach(var sql in new FormattableString[]{
                    $"UPDATE NhuCauTraoDoi SET GiaTriMongMuonTu=200, GiaTriMongMuonDen=100 WHERE TinDangId={ta.Id}",
                    $"UPDATE NhuCauTraoDoi SET TinhTrangToiThieu='Invalid' WHERE TinDangId={ta.Id}"})
                {
                    await using var tx=await db.Database.BeginTransactionAsync();var rejected=false;
                    try{await db.Database.ExecuteSqlInterpolatedAsync(sql);}catch(SqlException ex)when(ex.Number==547){rejected=true;}finally{await tx.RollbackAsync();}
                    Check(rejected,"SQL CHECK rejects invalid wished range/condition");
                }
                return true;
            });
            Check(await Db(db=>db.VongSmartSwap.CountAsync())==0,"All search tests ran without any persisted Smart Swap circle");
        }
        finally
        {
            if(factory!=null)await factory.DisposeAsync();
            if(created&&name.StartsWith("ReMarketTest_",StringComparison.Ordinal))await new SqlCommand($"ALTER DATABASE [{name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{name}]",master).ExecuteNonQueryAsync();
            if(Directory.Exists(folder)&&folder.StartsWith(tempRoot,StringComparison.OrdinalIgnoreCase)&&Path.GetFileName(folder)=="remarket-smart-swap-"+name)Directory.Delete(folder,true);
        }
    }
}
