using System.Drawing;

namespace Gemz.Api.Creator.Data.Model;

public class StoreTag : BaseDataModel
{
    public string Tagword { get; set; }
    public string CreatorId { get; set; }
    public DateTime CreatedOn { get; set; }
}