namespace Gemz.ServiceBus.Model
{
    public class StripeAccountMessageModel
    {
        public string StripeAccountId { get; set; }
        public string CreatorIdFromMetadata { get; set; }
        public bool DetailsSubmitted { get; set; }
        public bool ChargesEnabled { get; set; }
    }
}
