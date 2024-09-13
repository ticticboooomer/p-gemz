using MongoDB.Bson.Serialization.Attributes;

namespace Gemz.Api.Auth.Data.Model;

public class BaseDataModel
{
    [BsonId]
    public string Id { get; set; }
}