using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.ServiceBus.Model
{
    public class PaymentIntentMessageModel
    {
        public string PaymentIntentId { get; set; }

        public string MetadataOrderId { get; set; }
        public string MetadataCollectorId { get; set; }
        public long Amount { get; set; }
        public long ApplicationFeeAmount { get; set; }
        public string Currency { get; set; }
        public string LatestChargeId { get; set; }
        public string PaymentMethodId { get; set; }
        public string PaymentIntentClientSecret { get; set; }

        public DateTime CreatedOn { get; set; }
        public string EventType { get; set; }
    }
}
