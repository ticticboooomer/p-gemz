namespace Gemz.Api.Creator.Data.Model;

public class Image : BaseDataModel
{
    public string ContentType { get; set; }
    public string CreatorId { get; set; }
    public DateTime CreatedOn { get; set; }
    public bool Deleted { get; set; }   
}