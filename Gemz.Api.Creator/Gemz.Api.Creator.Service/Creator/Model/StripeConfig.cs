using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Creator.Service.Creator.Model
{
    public class StripeConfig
    {
        public string ApiKey { get; set; }
        public string RefreshUrl { get; set; }
        public string ReturnUrl { get; set; }
    }
}
