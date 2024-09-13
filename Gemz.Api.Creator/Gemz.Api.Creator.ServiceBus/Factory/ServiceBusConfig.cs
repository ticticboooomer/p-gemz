namespace Gemz.ServiceBus.Factory
{
    public class ServiceBusConfig
    {
        public string ConnectionString { get; set; }
        public string NotifyOrderQueueName { get; set; }

        public string StripeHandlerCreatorQueueName { get; set; }

        public bool ListenToNotifyOrderQueue { get; set; }
        public bool ListenToStripeCreatorQueue { get; set; }
    }
}
