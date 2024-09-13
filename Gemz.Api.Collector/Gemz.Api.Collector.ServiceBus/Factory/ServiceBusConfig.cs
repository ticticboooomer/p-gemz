using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.ServiceBus.Factory
{
    public class ServiceBusConfig
    {
        public string StripeHandlerQueueName { get; set; }
        public string NotifyOrderQueueName { get; set; }
        public bool ListenToStripeCollectorQueue { get; set; }
    }
}
