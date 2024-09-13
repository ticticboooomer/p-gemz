using System.Net.WebSockets;

namespace Gemz.Api.Creator.Model;

public class WebSocketModel
{
    public string CreatorId { get; set; }
    public WebSocket Socket { get; set; }
    public TaskCompletionSource Tcs { get; set; }
}
