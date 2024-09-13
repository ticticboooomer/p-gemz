namespace Gemz.Api.Collector.Data.Model
{
    public class StoresPage
    {
        public List<Store> Stores { get; set; }
        public int ThisPage { get; set; }
        public int TotalPages { get; set; }
    }
}
