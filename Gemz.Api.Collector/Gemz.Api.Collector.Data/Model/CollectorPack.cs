
namespace Gemz.Api.Collector.Data.Model
{
    public class CollectorPack : BaseDataModel
    {
        public string CollectorId { get; set; }
        public string CreatorId { get; set; }
        public string CollectionId { get; set; }
        public string OriginatingOrderId { get; set; }
        public string OriginatingOrderLineId { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
