using Microsoft.AspNetCore.SignalR;

namespace Charity_Kousar_Donation.Hubs;

public class DonationHub : Hub
{
    public async Task JoinAdmin() => await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
}

public interface IDonationNotifier
{
    Task NotifyDonationPaidAsync(string campaignTitle, decimal amount, string phone);
}

public class DonationNotifier(IHubContext<DonationHub> hub) : IDonationNotifier
{
    public Task NotifyDonationPaidAsync(string campaignTitle, decimal amount, string phone) =>
        hub.Clients.Group("admins").SendAsync("DonationPaid", new
        {
            campaignTitle,
            amount,
            phone = MaskPhone(phone),
            at = DateTime.UtcNow
        });

    private static string MaskPhone(string phone)
    {
        if (phone.Length < 8) return "***";
        return phone[..4] + "***" + phone[^4..];
    }
}
