namespace Charity_Kousar_Donation.Services;

public class UploadService(IWebHostEnvironment env, ILogger<UploadService> logger)
{
    private static readonly HashSet<string> Allowed = [".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg"];
    private const long MaxBytes = 5 * 1024 * 1024;

    public async Task<(bool Ok, string? Url, string? Error)> SaveImageAsync(IFormFile? file, HttpRequest request)
    {
        if (file is null || file.Length == 0) return (false, null, "فایلی انتخاب نشده است");
        if (file.Length > MaxBytes) return (false, null, "حداکثر حجم مجاز ۵ مگابایت است");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !Allowed.Contains(ext))
            return (false, null, "فرمت مجاز: jpg, png, webp, gif, svg");

        try
        {
            // WebRootPath can be null when wwwroot doesn't exist yet (e.g. fresh container) — fall back to content root.
            var webRoot = string.IsNullOrWhiteSpace(env.WebRootPath)
                ? Path.Combine(env.ContentRootPath, "wwwroot")
                : env.WebRootPath;

            var dir = Path.Combine(webRoot, "uploads");
            Directory.CreateDirectory(dir);

            var name = $"{Guid.NewGuid():N}{ext}";
            var path = Path.Combine(dir, name);

            await using (var stream = File.Create(path))
                await file.CopyToAsync(stream);

            // Return a root-relative URL so it works regardless of scheme/host/reverse-proxy
            // (avoids http/https mixed-content issues behind CapRover).
            return (true, $"/uploads/{name}", null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Image upload failed");
            return (false, null, "خطا در ذخیره فایل روی سرور");
        }
    }
}
