using System.Text.Json.Serialization;

namespace Gemz.Api.Auth.Service.Auth.Model;

public class TwitchApiUsersResponse
{
    [JsonPropertyName("data")]
    public List<Details> Data { get; set; }

    public class Details
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}