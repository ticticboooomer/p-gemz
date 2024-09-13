using Gemz.ServiceBus.Factory;
using MassTransit;
using Microsoft.Extensions.Options;

namespace Gemz.Api.Collector.Services;

public class StripeConsumerDefinition : ConsumerDefinition<StripeConsumer>
{
    public StripeConsumerDefinition(IOptions<ServiceBusConfig> config)
    {
        EndpointName = config.Value.StripeHandlerQueueName;
    }
}
