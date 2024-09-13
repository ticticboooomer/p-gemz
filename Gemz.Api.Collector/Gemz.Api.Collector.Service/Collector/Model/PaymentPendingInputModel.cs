using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Collector.Service.Collector.Model
{
    public class PaymentPendingInputModel
    {
        public string PaymentIntentClientSecret { get; set; }
        public string StripeErrorMessage { get; set; }
    }
}
