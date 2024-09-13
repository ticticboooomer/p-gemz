using System.Collections.Concurrent;
using Gemz.ServiceBus.Model;
using MassTransit;

namespace Gemz.Api.Creator.Services;

public class NotifyConsumer : IConsumer<NotifyOrderModel>
{
    private readonly NotifyHostedService _nhs;

    public NotifyConsumer(NotifyHostedService nhs)
    {
        _nhs = nhs;
    }

    public async Task Consume(ConsumeContext<NotifyOrderModel> context)
    {
        await _nhs.Consume(context.Message);
    }
}
