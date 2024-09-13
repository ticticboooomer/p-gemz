using Gemz.Api.Creator.Services;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Net.WebSockets;
using System.Text;
using Gemz.Api.Creator.Service.Creator;

namespace Gemz.Api.Creator.Middleware;

public class OverlayWsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly IOverlayService _ovService;
    private readonly NotifyHostedService _service;

    public OverlayWsService(HttpClient httpClient, IConfiguration config, IOverlayService ovService, NotifyHostedService service)
    {
        _httpClient = httpClient;
        _config = config;
        _ovService = ovService;
        _service = service;
    }

    public async Task AwaitAuth(WebSocket socket, TaskCompletionSource tcs)
    {
        var buffer = new byte[1024 * 32];
        var res = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        var key = Encoding.UTF8.GetString(buffer, 0, res.Count);
        var validatedKey = await _ovService.ValidateKey(key);

        if (validatedKey == null)
        {
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            return;
        }


        _service.AddSocket(validatedKey.CreatorId, socket, tcs);
    }
}
