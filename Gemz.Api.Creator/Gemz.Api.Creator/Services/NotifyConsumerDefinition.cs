using Gemz.ServiceBus.Factory;
using MassTransit;
using Microsoft.Extensions.Options;

namespace Gemz.Api.Creator.Services;

public class NotifyConsumerDefinition : ConsumerDefinition<NotifyConsumer>
{
    public NotifyConsumerDefinition(IOptions<ServiceBusConfig> config)
    {
        EndpointName = config.Value.NotifyOrderQueueName;
    }
}
