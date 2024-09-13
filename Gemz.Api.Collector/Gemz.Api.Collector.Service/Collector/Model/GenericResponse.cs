namespace Gemz.Api.Collector.Service.Collector.Model;

public class GenericResponse<T>
{
    public T? Data { get; set; }
    public string? Error { get; set; }
}