using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.StripeWebhook.Service.StripeWebhook.Model
{
    public class StripeConfig
    {
        public string? ApiKey { get; set; }
        public string? WebHookEndpointSecret { get; set; }

    }
}
