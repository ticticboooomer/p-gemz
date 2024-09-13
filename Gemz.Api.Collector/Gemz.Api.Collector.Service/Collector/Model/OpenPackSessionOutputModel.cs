
namespace Gemz.Api.Collector.Service.Collector.Model
{
    public class OpenPackSessionOutputModel
    {
        public string SessionId { get; set; }
        public string CollectionId { get; set; }
        public string CollectionName { get; set; }
        public List<OpenPackSessionGemOutputModel> OpenedGems { get; set; }
        public int GemsForCreatorToOpen { get; set; }
    }
}
