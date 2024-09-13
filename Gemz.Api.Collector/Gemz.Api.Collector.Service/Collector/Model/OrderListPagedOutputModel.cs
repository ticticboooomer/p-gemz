namespace Gemz.Api.Collector.Service.Collector.Model
{
    public class OrderListPagedOutputModel
    {
        public List<OrderHeaderOutputModel> Orders { get; set; }
        public int ThisPage { get; set; }
        public int TotalPages { get; set; }
    }
}
