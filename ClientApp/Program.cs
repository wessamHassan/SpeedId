// See https://aka.ms/new-console-template for more information
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.WebSockets;
using System.Text;

Console.WriteLine("Start /messages Connection...");
Console.WriteLine("==============================");
await CallingMessages();
Console.WriteLine("\n");


Console.WriteLine("Start /server/ping Connection...");
Console.WriteLine("==============================");
await CallingPing();
Console.WriteLine("\n");

Console.WriteLine("Start /work/start Connection...");
Console.WriteLine("==============================");
await CallingWork();

await Task.Delay(1000);
Console.Write("Press any key to close ...");
Console.ReadKey();

static async Task CallingMessages()
{
    Uri uri = new("ws://localhost:2910/messages");

    using (ClientWebSocket webSocket = new())
    {
        await webSocket.ConnectAsync(uri, CancellationToken.None);
        byte[] buffer = new byte[1024];

        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
            else
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"{message}");
            }
        }
       
    }
}
static async Task CallingPing()
{
    var httpClient = new HttpClient();
    var pingResponse = await httpClient.PostAsync("http://localhost:2910/server/ping", null);
    if (pingResponse.IsSuccessStatusCode)
    {
        var pingResponseContent = await pingResponse.Content.ReadAsStringAsync();
        Console.WriteLine(pingResponseContent);

    }
    else
    {
        Console.WriteLine("HTTP request to /server/ping failed with status code: " + pingResponse.StatusCode);
    }
}
static async Task CallingWork()
{
    //create a connection to the server
    var connection = new HubConnectionBuilder().WithUrl("http://localhost:2910/work").Build();
    //connect to the method that hub invokes
    //defin an event handler for the “Response” event
    //Response has 2 string args
    connection.On<string,string>("Response", (message, time) =>
    {
        Console.WriteLine($"{message} at: {time}");
    });
    //start the connection
    await connection.StartAsync();

    // send the HTTP POST request
    using (var client = new HttpClient())
    {
        await client.PostAsync("http://localhost:2910/work/start", null);
    }
}


