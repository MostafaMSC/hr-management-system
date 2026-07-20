using Microsoft.AspNetCore.SignalR;

namespace HR.Infrastructure.Hubs;

public class RealTimeHub : Hub
{
    public async Task SendUpdate(string message)
    {
        await Clients.All.SendAsync("ReceiveUpdate", message);
    }
}
