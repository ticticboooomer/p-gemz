using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.ServiceBus.Model
{
    public class ServiceBusConfig
    {
        public string ConnectionString { get; set; }
        public string StripeHandlerCollectorQueueName { get; set; }
        public string StripeHandlerCreatorQueueName { get; set; }

    }

}
