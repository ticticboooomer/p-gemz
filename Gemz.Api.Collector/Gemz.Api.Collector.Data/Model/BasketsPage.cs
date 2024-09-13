namespace Gemz.Api.Collector.Data.Model
{
    public class BasketsPage
    {
        public List<Basket> Baskets { get; set; }
        public int ThisPage { get; set; }
        public int TotalPages { get; set; }
    }
}
