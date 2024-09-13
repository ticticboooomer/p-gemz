using Gemz.Api.Creator.Data.Model;

namespace Gemz.Api.Creator.Service.Creator.Model;

public class CreatorCollections
{
    public List<CollectionModel> Collections { get; set; }
    public int ThisPage { get; set; }
    public int TotalPages { get; set; }

}