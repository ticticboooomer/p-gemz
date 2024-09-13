namespace Gemz.Api.Collector.Service.Collector.Model;

public class GemSampleWithCountModel
{
    public int TotalGemsInCollection { get; set; }
    public List<GemModel> Gems { get; set; }
}