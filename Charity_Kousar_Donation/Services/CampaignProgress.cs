namespace Charity_Kousar_Donation.Services;

/// <summary>
/// How far a project has come. One place, so the list, the detail page and the
/// share text can never disagree about a campaign's numbers.
/// </summary>
public static class CampaignProgress
{
    /// <summary>Whole percent of the goal that has been raised, capped at 100.</summary>
    public static int Percent(decimal target, decimal collected) =>
        target > 0 ? (int)Math.Min(100, Math.Max(0, collected) / target * 100) : 0;

    /// <summary>A project counts as completed once its goal is reached.</summary>
    public static bool IsCompleted(decimal target, decimal collected) =>
        target > 0 && collected >= target;
}
