using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using signalr.Dto;

namespace signalr;

public class ChatHub : Hub
{
    private static readonly List<ConnectedUserDto> _connections = new();
    
    public override Task OnConnectedAsync()
    {
        var connectionInfo = new ConnectedUserDto(Context.ConnectionId, $"user_{new Random().Next()}");
        _connections.Add(connectionInfo);
        Clients.Client(Context.ConnectionId).SendAsync("ReceiveConnectionInfo", JsonConvert.SerializeObject(connectionInfo));
        Clients.All.SendAsync("UpdateConnectedUsers", JsonConvert.SerializeObject(_connections));

        return base.OnConnectedAsync();
    }


    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var connection = _connections.Find(c => c.ConnectionId == Context.ConnectionId);
        if(connection != null)
            _connections.Remove(connection);
        Clients.All.SendAsync("UpdateConnectedUsers", JsonConvert.SerializeObject(_connections));

        return base.OnDisconnectedAsync(exception);
    }


    public async Task SendMessage(string user, string message, string receiverId)
    {
        if(receiverId == "0")
            await Clients.All.SendAsync("ReceiveMessage", user, message, false, Context.ConnectionId);

        await Clients.Client(receiverId).SendAsync("ReceiveMessage", user, message, true, false, Context.ConnectionId);
    }
}