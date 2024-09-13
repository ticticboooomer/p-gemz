
namespace Gemz.Api.Collector.Service.Collector.Model
{
    public class StoresPurchasedFromOutputModel
    {
        public string StoreName { get; set; }
        public string StoreTag { get; set; }
        public string StoreLogoImageId { get; set; }
        public int GemCount { get; set; }
        public int CollectionCount { get; set; }
    }
}
