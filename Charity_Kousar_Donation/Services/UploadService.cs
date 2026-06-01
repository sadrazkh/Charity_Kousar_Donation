namespace Charity_Kousar_Donation.Services;

public class UploadService(IWebHostEnvironment env, ILogger<UploadService> logger)
{
    private static readonly HashSet<string> Allowed = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private const long MaxBytes = 5 * 1024 * 1024;

    public async Task<(bool Ok, string? Url, string? Error)> SaveImageAsync(IFormFile file, HttpRequest request)
    {
        if (file.Length == 0) return (false, null, "فایل خالی است");
        if (file.Length > MaxBytes) return (false, null, "حداکثر حجم ۵ مگابایت");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Allowed.Contains(ext)) return (false, null, "فرمت مجاز: jpg, png, webp, gif");

        var dir = Path.Combine(env.WebRootPath, "uploads");
        Directory.CreateDirectory(dir);
        var name = $"{Guid.NewGuid():N}{ext}";
        var path = Path.Combine(dir, name);

        await using var stream = File.Create(path);
        await file.CopyToAsync(stream);

        var baseUrl = $"{request.Scheme}://{request.Host}";
        return (true, $"{baseUrl}/uploads/{name}", null);
    }
}
