using Gemz.ServiceBus.Factory;
using MassTransit;
using Microsoft.Extensions.Options;

namespace Gemz.Api.Creator.Services;

public class StripeAccountConsumerDefinition : ConsumerDefinition<StripeAccountConsumer>
{
    public StripeAccountConsumerDefinition(IOptions<ServiceBusConfig> config)
    {
        EndpointName = config.Value.StripeHandlerCreatorQueueName;
    }
}
