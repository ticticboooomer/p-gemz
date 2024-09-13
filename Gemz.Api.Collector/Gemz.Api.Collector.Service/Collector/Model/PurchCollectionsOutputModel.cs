using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Collector.Service.Collector.Model
{
    public class PurchCollectionsOutputModel
    {
        public string CollectionId { get; set; }
        public string CollectionName { get; set; }
        public int GemCount { get; set; }
        public int CreatorToOpenCount { get; set; }
        public List<PurchCollectionGemsOutputModel> GemSelection { get; set; }
    }
}
