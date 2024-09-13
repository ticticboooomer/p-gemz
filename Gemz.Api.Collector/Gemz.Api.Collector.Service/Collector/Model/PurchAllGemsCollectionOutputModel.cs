
namespace Gemz.Api.Collector.Service.Collector.Model
{
    public class PurchAllGemsCollectionOutputModel
    {
        public string StoreName { get; set; }
        public string StoreTag { get; set; }
        public string StoreLogoImageId { get; set; }
        public string CollectionId { get; set; }
        public string CollectionName { get; set; }
        public int GemsForCreatorToOpen { get; set; }
        public List<PurchGemOutputModel> PurchasedGems { get; set; }
    }
}
