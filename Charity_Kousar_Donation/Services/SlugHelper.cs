using System.Text;
using System.Text.RegularExpressions;

namespace Charity_Kousar_Donation.Services;

public static partial class SlugHelper
{
    private const string ShortChars = "abcdefghijklmnopqrstuvwxyz23456789";

    private static readonly (string From, string To)[] PersianMap =
    [
        ("آ", "a"), ("ا", "a"), ("ب", "b"), ("پ", "p"), ("ت", "t"), ("ث", "s"),
        ("ج", "j"), ("چ", "ch"), ("ح", "h"), ("خ", "kh"), ("د", "d"), ("ذ", "z"),
        ("ر", "r"), ("ز", "z"), ("ژ", "zh"), ("س", "s"), ("ش", "sh"), ("ص", "s"),
        ("ض", "z"), ("ط", "t"), ("ظ", "z"), ("ع", "a"), ("غ", "gh"), ("ف", "f"),
        ("ق", "gh"), ("ک", "k"), ("ك", "k"), ("گ", "g"), ("ل", "l"), ("م", "m"),
        ("ن", "n"), ("و", "o"), ("ه", "h"), ("ی", "y"), ("ئ", "y"), (" ", "-")
    ];

    public static string ToSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return Guid.NewGuid().ToString("N")[..8];
        var normalized = text.Trim().ToLowerInvariant();
        foreach (var (from, to) in PersianMap)
            normalized = normalized.Replace(from, to);
        normalized = NonAlphanumeric().Replace(normalized, "-");
        normalized = MultiDash().Replace(normalized, "-").Trim('-');
        return string.IsNullOrEmpty(normalized) ? Guid.NewGuid().ToString("N")[..8] : normalized;
    }

    public static string GenerateShortCode(int length = 6)
    {
        var sb = new StringBuilder(length);
        for (var i = 0; i < length; i++)
            sb.Append(ShortChars[Random.Shared.Next(ShortChars.Length)]);
        return sb.ToString();
    }

    [GeneratedRegex(@"[^a-z0-9\-]")]
    private static partial Regex NonAlphanumeric();

    [GeneratedRegex(@"-+")]
    private static partial Regex MultiDash();
}
