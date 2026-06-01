namespace Charity_Kousar_Donation.Models;

public enum SettingType
{
    Text,
    Password,
    Number,
    Boolean,
    Color,
    Url,
    TextArea
}

public class SiteSetting
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
    public string Group { get; set; } = "general";
    public string LabelFa { get; set; } = "";
    public string LabelEn { get; set; } = "";
    public SettingType Type { get; set; } = SettingType.Text;
    public int SortOrder { get; set; }
}
