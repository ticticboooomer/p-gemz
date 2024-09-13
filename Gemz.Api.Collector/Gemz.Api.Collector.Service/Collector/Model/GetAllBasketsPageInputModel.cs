namespace Gemz.Api.Collector.Service.Collector.Model
{
    public class GetAllBasketsPageInputModel
    {
        public string StoreTagToExclude { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }
}
