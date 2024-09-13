using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Collector.Data.Model
{
    public class CollectorPackOpened : BaseDataModel
    {
        public string CollectorId { get; set; }
        public string CreatorId { get; set; }
        public string CollectionId { get; set; }
        public string OriginatingOrderId { get; set; }
        public string OriginatingOrderLineId { get; set; }
        public List<string> GemsCreatedFromPack { get; set; }
        public DateTime CreatedOn { get; set; }

    }
}
