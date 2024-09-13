using MongoDB.Bson.Serialization.Attributes;

namespace Gemz.Api.Creator.Data.Model;

public class BaseDataModel
{
    [BsonId]
    public string Id { get; set; }

    public string id => Id;
}