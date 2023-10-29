using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using ServerApp.Hubs;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();

builder.WebHost.UseUrls("http://localhost:2910");
var app = builder.Build();
app.UseWebSockets();


app.Map("/messages", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

        var buffer = Encoding.UTF8.GetBytes("===> Welcome");
        var segment = new ArraySegment<byte>(buffer);
        await webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);     

        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
    }
    else
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }

});

app.MapPost("/server/ping", async context => {

    var responseMessage = "===> Pong";
    await context.Response.WriteAsync(responseMessage);

});

app.MapPost("/work/start", async context =>
{
    var hubContext = context.RequestServices.GetRequiredService<IHubContext<WorkHub>>();
    string id = Guid.NewGuid().ToString();

    await hubContext.Clients.All.SendAsync("Response", $"===> Work Started, Id: {id}", DateTime.Now.ToString("HH:mm:ss "));

    await Task.Delay(5000);

    await hubContext.Clients.All.SendAsync("Response", $"===> Work Completed, Id: {id}", DateTime.Now.ToString("HH:mm:ss "));
});

app.MapHub<WorkHub>("/work");
await app.RunAsync();


