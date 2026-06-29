namespace Charity_Kousar_Donation.Services;

public class UploadService(IWebHostEnvironment env, IConfiguration config, ILogger<UploadService> logger)
{
    private static readonly HashSet<string> Allowed = [".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg"];
    private const long MaxBytes = 5 * 1024 * 1024;

    /// <summary>
    /// Physical directory where uploads are stored. Configurable via "Uploads:Path"
    /// (set it to a persistent volume on CapRover, e.g. /app/uploads). Defaults to a
    /// writable folder under the content root rather than wwwroot (which the non-root
    /// container user often cannot write to).
    /// </summary>
    public static string ResolveUploadsDir(IWebHostEnvironment env, IConfiguration config)
    {
        var configured = config["Uploads:Path"];
        if (!string.IsNullOrWhiteSpace(configured)) return configured;
        return Path.Combine(env.ContentRootPath, "uploads");
    }

    public async Task<(bool Ok, string? Url, string? Error)> SaveImageAsync(IFormFile? file, HttpRequest request)
    {
        if (file is null || file.Length == 0) return (false, null, "فایلی انتخاب نشده است");
        if (file.Length > MaxBytes) return (false, null, "حداکثر حجم مجاز ۵ مگابایت است");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !Allowed.Contains(ext))
            return (false, null, "فرمت مجاز: jpg, png, webp, gif, svg");

        try
        {
            var dir = ResolveUploadsDir(env, config);
            Directory.CreateDirectory(dir);

            var name = $"{Guid.NewGuid():N}{ext}";
            var path = Path.Combine(dir, name);

            await using (var stream = File.Create(path))
                await file.CopyToAsync(stream);

            // Root-relative URL — served from the uploads static-file mapping in Program.cs.
            return (true, $"/uploads/{name}", null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Image upload failed (dir={Dir})", ResolveUploadsDir(env, config));
            // Surface the real reason to the (admin-only) caller to ease diagnosing CapRover issues.
            return (false, null, $"خطا در ذخیره فایل روی سرور: {ex.Message}");
        }
    }
}
