using Charity_Kousar_Donation.Data;
using Charity_Kousar_Donation.Models;

namespace Charity_Kousar_Donation.Tests;

public class SettingsCatalogTests
{
    [Fact]
    public void Keys_are_unique()
    {
        var duplicates = SettingsCatalog.All
            .GroupBy(d => d.Key)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void Every_setting_has_a_key_group_and_labels_for_the_admin_panel()
    {
        foreach (var d in SettingsCatalog.All)
        {
            Assert.False(string.IsNullOrWhiteSpace(d.Key), $"a setting has no key");
            Assert.False(string.IsNullOrWhiteSpace(d.Group), $"{d.Key} has no group");
            Assert.False(string.IsNullOrWhiteSpace(d.LabelFa), $"{d.Key} has no Persian label");
            Assert.False(string.IsNullOrWhiteSpace(d.LabelEn), $"{d.Key} has no English label");
        }
    }

    [Theory]
    [InlineData(SettingType.Boolean)]
    [InlineData(SettingType.Number)]
    public void Typed_settings_have_a_default_the_client_can_parse(SettingType type)
    {
        foreach (var d in SettingsCatalog.All.Where(x => x.Type == type))
        {
            var ok = type == SettingType.Boolean
                ? bool.TryParse(d.Default, out _)
                : decimal.TryParse(d.Default, out _);
            Assert.True(ok, $"{d.Key} is a {type} but its default is \"{d.Default}\"");
        }
    }

    [Fact]
    public void Color_settings_are_blank_or_a_hex_value()
    {
        foreach (var d in SettingsCatalog.All.Where(x => x.Type == SettingType.Color))
            Assert.True(d.Default.Length == 0 || System.Text.RegularExpressions.Regex.IsMatch(d.Default, "^#[0-9a-fA-F]{6}$"),
                $"{d.Key} has a non-hex colour default \"{d.Default}\"");
    }

    [Fact]
    public void DefaultOf_returns_the_catalog_value_and_blank_for_unknown_keys()
    {
        Assert.Equal("shimmer", SettingsCatalog.DefaultOf("site.progress.flow.style"));
        Assert.Equal(string.Empty, SettingsCatalog.DefaultOf("no.such.setting"));
    }

    [Fact]
    public void Rows_built_for_the_database_carry_the_catalog_values()
    {
        var def = SettingsCatalog.All.First(d => d.Key == "site.name.fa");
        var row = def.ToRow();

        Assert.Equal(def.Key, row.Key);
        Assert.Equal(def.Default, row.Value);
        Assert.Equal(def.Group, row.Group);
        Assert.Equal(def.LabelFa, row.LabelFa);
        Assert.Equal(def.LabelEn, row.LabelEn);
        Assert.Equal(def.Type, row.Type);
        Assert.Equal(def.SortOrder, row.SortOrder);
    }
}
