namespace Gemz.Api.Collector.Service.Collector.Model
{
    public class BasketPagedOutputModel
    {
        public List<BasketIdOutputModel> Baskets { get; set; }
        public int ThisPage { get; set; }
        public int TotalPages { get; set; }

    }
}
