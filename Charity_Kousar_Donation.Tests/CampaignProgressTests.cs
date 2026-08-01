using Charity_Kousar_Donation.Services;

namespace Charity_Kousar_Donation.Tests;

public class CampaignProgressTests
{
    [Theory]
    [InlineData(0, 0, 0)]           // no goal set → nothing to show
    [InlineData(0, 5_000, 0)]       // money raised without a goal is still 0%
    [InlineData(100, 0, 0)]
    [InlineData(100, 25, 25)]
    [InlineData(100, 99.9, 99)]     // truncates, so 99.9% never reads as "100%"
    [InlineData(100, 100, 100)]
    [InlineData(100, 250, 100)]     // over-funded stays capped
    [InlineData(3, 1, 33)]
    public void Percent_is_capped_and_never_negative(decimal target, decimal collected, int expected) =>
        Assert.Equal(expected, CampaignProgress.Percent(target, collected));

    [Theory]
    [InlineData(0, 0, false)]           // a project with no goal can never be "completed"
    [InlineData(0, 1_000, false)]
    [InlineData(50_000, 49_999, false)]
    [InlineData(50_000, 50_000, true)]  // exactly on the goal counts
    [InlineData(50_000, 60_000, true)]
    public void IsCompleted_needs_a_goal_that_is_reached(decimal target, decimal collected, bool expected) =>
        Assert.Equal(expected, CampaignProgress.IsCompleted(target, collected));

    [Fact]
    public void A_completed_campaign_always_shows_a_full_bar()
    {
        foreach (var (target, collected) in new[] { (100m, 100m), (100m, 120m), (7m, 7m) })
        {
            Assert.True(CampaignProgress.IsCompleted(target, collected));
            Assert.Equal(100, CampaignProgress.Percent(target, collected));
        }
    }
}
