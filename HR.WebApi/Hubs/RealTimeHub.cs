using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace HR.WebApi.Hubs;

// Add authorization if needed. For now it is just an open hub.
public class RealTimeHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
