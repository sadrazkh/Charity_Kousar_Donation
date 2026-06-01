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
}
