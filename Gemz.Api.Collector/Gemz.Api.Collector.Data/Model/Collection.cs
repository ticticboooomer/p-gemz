namespace Gemz.Api.Collector.Data.Model;

public class Collection : BaseDataModel
{
    public string CreatorId { get; set; }
    public string Name { get; set; }
    public int PublishedStatus { get; set; }
    public float ClusterPrice { get; set; }
    public DateTime CreatedOn { get; set; }
    public bool Deleted { get; set; }
}