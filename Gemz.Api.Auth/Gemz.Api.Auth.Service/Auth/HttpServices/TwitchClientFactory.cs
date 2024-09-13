using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gemz.Api.Auth.Service.Auth.HttpServices;

public class TwitchClientFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TwitchClientFactory> _logger;

    public TwitchClientFactory(IServiceProvider serviceProvider, ILogger<TwitchClientFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public TwitchClient Create()
    {
        _logger.LogDebug("Entered Create for TwitchClientFactory");
        return _serviceProvider.GetRequiredService<TwitchClient>();
    }
}