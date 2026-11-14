namespace ReMarket.Infrastructure.Authentication;

public sealed class TuyChonJwt
{
    public string Issuer { get; set; } = "ReMarket";
    public string Audience { get; set; } = "ReMarket.Android";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessMinutes { get; set; } = 15;
    public int RefreshDays { get; set; } = 7;

    public byte[] KhoaKy()
    {
        byte[] key;
        try { key = Convert.FromBase64String(SigningKey); }
        catch (FormatException) { throw new InvalidOperationException("Jwt:SigningKey phai la Base64 cua it nhat 32 byte ngau nhien."); }
        if (key.Length < 32 || AccessMinutes is < 1 or > 60 || RefreshDays is < 1 or > 30)
            throw new InvalidOperationException("Cau hinh JWT khong hop le. Chay Initialize-Authentication.ps1 trong Development.");
        return key;
    }
}
