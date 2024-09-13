namespace Gemz.Api.Creator.Service.Creator.Model;

public class CollectionModel
{
    public string Id { get; set; }
    public string CreatorId { get; set; }
    public string Name { get; set; }
    public float ClusterPrice { get; set; }
    public int PublishedStatus { get; set; }
    public DateTime CreatedOn { get; set; }
    public bool Deleted { get; set; }
    public bool PublishDenied { get; set; }
}