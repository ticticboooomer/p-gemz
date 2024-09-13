
namespace Gemz.Api.Collector.Service.Collector.Model
{
    public class UnopenedPackOutputModel
    {
        public string CreatorStoreName { get; set; }
        public string CreatorStoreTag { get; set; }
        public string CreatorLogoImageId { get; set; }
        public List<UnopenedPackCollections> UnopenedPackCollections { get; set; }
    }
}
