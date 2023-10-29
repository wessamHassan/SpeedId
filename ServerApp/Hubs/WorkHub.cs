using Microsoft.AspNetCore.SignalR;

namespace ServerApp.Hubs
{
    public class WorkHub : Hub
    {
        public async Task SendMessage(string timestamp, string message)
        {
            //it sends the message to all connected clients.
            //Response is the method that will be invoked on the client side, which has 2 string args
            await Clients.All.SendAsync("Response", message, timestamp);
        }
    }
}
