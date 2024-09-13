namespace Gemz.Api.Collector.Data.Model
{
    public class OrderPage
    {
        public List<Order> Orders { get; set; }
        public int ThisPage { get; set; }
        public int TotalPages { get; set; }
    }
}
