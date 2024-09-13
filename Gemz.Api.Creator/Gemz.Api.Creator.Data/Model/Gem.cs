namespace Gemz.Api.Creator.Data.Model;

public class Gem : BaseDataModel
{
    public string CollectionId { get; set; }
    public string CreatorId { get; set; }
    public string Name { get; set; }
    public int Rarity { get; set; }
    public string ImageId { get; set; }
    public int SizePercentage { get; set; }
    public int PositionXPercentage { get; set; }
    public int PositionYPercentage { get; set; }
    public int PublishedStatus { get; set; }
    public DateTime CreatedOn { get; set; }
    public bool Deleted { get; set; }

}