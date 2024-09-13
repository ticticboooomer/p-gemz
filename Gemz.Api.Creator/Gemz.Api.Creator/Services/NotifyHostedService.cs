using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Gemz.Api.Creator.Model;
using Gemz.ServiceBus.Factory;
using Gemz.ServiceBus.Model;
using Microsoft.Extensions.Options;

namespace Gemz.Api.Creator.Services;

public class NotifyHostedService : IHostedService
{
    private readonly ILogger<NotifyHostedService> _logger;
    private readonly IOptions<ServiceBusConfig> _serviceBusConfig;
    private readonly Dictionary<string, List<WebSocketModel>> _webSockets = new();
    private readonly Timer _timer;

    public NotifyHostedService(ILogger<NotifyHostedService> logger,
        IOptions<ServiceBusConfig> serviceBusConfig)
    {
        _logger = logger;
        _serviceBusConfig = serviceBusConfig;
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));

    }

    private void DoWork(object state)
    {
        foreach (var kv in _webSockets)
        {
            var remove = new List<WebSocketModel>();
            if (kv.Value == null)
            {
                continue;
            }

            foreach (var model in kv.Value)
            {
                if (model.Socket.State != WebSocketState.Open)
                {
                    remove.Add(model);
                }
            }

            foreach (var model in remove)
            {
                kv.Value.Remove(model);
            }
        }
    }

    public async Task Consume(NotifyOrderModel data)
    {
        if (!_serviceBusConfig.Value.ListenToNotifyOrderQueue)
        {
            _logger.LogDebug("ListenToNotifyOrderQueue config setting is false. NotifyHostedService not running. Leaving function.");
            return;
        }

        _logger.LogInformation("Received Queue message to notify creator overlay of order");
        if (_webSockets.TryGetValue(data.CreatorId ?? string.Empty, out var model))
        {
            async Task Action(WebSocketModel x)
            {
                if (x.Socket.State == WebSocketState.Open)
                {
                    var buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
                    await x.Socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }

            foreach (var wsm in model)
            {
                await Action(wsm);
            }
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Entered StartAsync in NotifyHostedService");


        if (!_serviceBusConfig.Value.ListenToNotifyOrderQueue)
        {
            _logger.LogDebug("ListenToNotifyOrderQueue config setting is false. NotifyHostedService not running. Leaving function.");
            return;
        }
        _logger.LogDebug("NotifyHostedService has started running");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }

    public void AddSocket(string sub, WebSocket socket, TaskCompletionSource tcs)
    {
        var model = new WebSocketModel()
        {
            CreatorId = sub,
            Socket = socket,
            Tcs = tcs
        };
        if (!_webSockets.ContainsKey(sub))
        {
            _webSockets[sub] = new List<WebSocketModel>()
            {
                model
            };
        }
        else
        {
            _webSockets[sub].Add(model);
        }
    }
}