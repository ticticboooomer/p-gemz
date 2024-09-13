
namespace Gemz.Api.Collector.Data.Model
{
    public class OpenPackSession : BaseDataModel
    {
        public string CollectorId { get; set; }
        public string CollectionId { get; set; }
        public string CollectionName { get; set; }
        public List<OpenPackSessionGem> OpenedGems { get; set; }
        public int GemsForCreatorToOpen { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
