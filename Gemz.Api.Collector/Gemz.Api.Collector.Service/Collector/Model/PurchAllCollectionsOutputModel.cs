using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Collector.Service.Collector.Model
{
    public class PurchAllCollectionsOutputModel
    {
        public string StoreName { get; set; }
        public string StoreLogoImageId { get; set; }
        public List<PurchCollectionsOutputModel> Collections { get; set; }

    }
}
