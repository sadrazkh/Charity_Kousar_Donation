using System.Text.Json;
using System.Text.Json.Serialization;
using Charity_Kousar_Donation.Models;

namespace Charity_Kousar_Donation.Services;

public static class CampaignPageHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static List<CampaignPageBlock> ParseBlocks(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try
        {
            return JsonSerializer.Deserialize<List<CampaignPageBlock>>(json, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public static string SerializeBlocks(List<CampaignPageBlock> blocks) =>
        JsonSerializer.Serialize(blocks, JsonOptions);

    public static List<CampaignPageBlock> CreateDefaultBlocks(Campaign c)
    {
        var blocks = new List<CampaignPageBlock>();

        if (!string.IsNullOrWhiteSpace(c.ImageUrl))
        {
            blocks.Add(new CampaignPageBlock
            {
                Type = "image",
                Data = new Dictionary<string, object?>
                {
                    ["url"] = c.ImageUrl,
                    ["captionFa"] = "",
                    ["captionEn"] = "",
                    ["fullWidth"] = true
                }
            });
        }

        blocks.Add(new CampaignPageBlock
        {
            Type = "heading",
            Data = new Dictionary<string, object?>
            {
                ["textFa"] = c.TitleFa,
                ["textEn"] = c.TitleEn,
                ["level"] = 1
            }
        });

        if (!string.IsNullOrWhiteSpace(c.DescriptionFa) || !string.IsNullOrWhiteSpace(c.DescriptionEn))
        {
            blocks.Add(new CampaignPageBlock
            {
                Type = "text",
                Data = new Dictionary<string, object?>
                {
                    ["contentFa"] = c.DescriptionFa,
                    ["contentEn"] = c.DescriptionEn
                }
            });
        }

        blocks.Add(new CampaignPageBlock { Type = "stats", Data = [] });
        blocks.Add(new CampaignPageBlock
        {
            Type = "cta",
            Data = new Dictionary<string, object?>
            {
                ["textFa"] = "همین حالا کمک کنید",
                ["textEn"] = "Donate now"
            }
        });

        return blocks;
    }

    public static string ExtractPlainText(List<CampaignPageBlock> blocks, bool fa)
    {
        var parts = new List<string>();
        foreach (var b in blocks)
            CollectText(b, fa, parts);
        return string.Join("\n", parts.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()));
    }

    private static void CollectText(CampaignPageBlock block, bool fa, List<string> parts)
    {
        var d = block.Data;
        if (d == null) return;

        string? pick(params string[] keys)
        {
            foreach (var k in keys)
                if (d.TryGetValue(k, out var v) && v != null)
                {
                    var s = v.ToString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(s)) return s;
                }
            return null;
        }

        switch (block.Type)
        {
            case "heading":
            case "text":
            case "quote":
            case "cta":
                var t = pick(fa ? "textFa" : "textEn", fa ? "contentFa" : "contentEn", "textFa", "textEn", "contentFa", "contentEn");
                if (t != null) parts.Add(t);
                break;
            case "columns":
                if (d.TryGetValue("columns", out var cols))
                    WalkNestedBlocks(cols, fa, parts);
                break;
            case "section":
                if (d.TryGetValue("blocks", out var sb))
                    WalkNestedBlocks(sb, fa, parts);
                break;
        }
    }

    private static void WalkNestedBlocks(object? value, bool fa, List<string> parts)
    {
        if (value is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in je.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var child in item.EnumerateArray())
                        {
                            var childBlock = JsonSerializer.Deserialize<CampaignPageBlock>(child.GetRawText(), JsonOptions);
                            if (childBlock != null) CollectText(childBlock, fa, parts);
                        }
                    }
                    else if (item.ValueKind == JsonValueKind.Object)
                    {
                        var childBlock = JsonSerializer.Deserialize<CampaignPageBlock>(item.GetRawText(), JsonOptions);
                        if (childBlock != null) CollectText(childBlock, fa, parts);
                    }
                }
            }
        }
        else if (value is IEnumerable<object?> list)
        {
            foreach (var item in list)
            {
                if (item is IEnumerable<object?> inner && item is not string)
                {
                    foreach (var child in inner)
                    {
                        if (child is CampaignPageBlock cb) CollectText(cb, fa, parts);
                        else if (child is JsonElement el && el.ValueKind == JsonValueKind.Object)
                        {
                            var childBlock = JsonSerializer.Deserialize<CampaignPageBlock>(el.GetRawText(), JsonOptions);
                            if (childBlock != null) CollectText(childBlock, fa, parts);
                        }
                    }
                }
                else if (item is CampaignPageBlock cb) CollectText(cb, fa, parts);
            }
        }
    }
}
