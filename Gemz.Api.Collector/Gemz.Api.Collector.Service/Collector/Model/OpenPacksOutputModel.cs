
namespace Gemz.Api.Collector.Service.Collector.Model
{
    public class OpenPacksOutputModel
    {
        public string CollectionId { get; set; }
        public string CollectionName { get; set; }
        public List<OpenPacksGem> OpenedGems { get; set; }
        public int GemsForCreatorToOpen { get; set; }
    }
}
