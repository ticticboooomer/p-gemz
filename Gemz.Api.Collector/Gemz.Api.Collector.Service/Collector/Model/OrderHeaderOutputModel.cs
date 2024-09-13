using Gemz.Api.Collector.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Collector.Service.Collector.Model
{
    public class OrderHeaderOutputModel
    {
        public string OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public double OrderTotal { get; set; }
        public string OrderDate { get; set; }
        public string OrderTime { get; set; }
        public string StoreName { get; set; }
        public string StoreLogoImageId { get; set; }
        public string StoreTag { get; set; }
    }
}
