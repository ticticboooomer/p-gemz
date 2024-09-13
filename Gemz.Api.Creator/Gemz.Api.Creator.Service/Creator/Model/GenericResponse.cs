namespace Gemz.Api.Creator.Service.Creator.Model;

public class GenericResponse<T>
{
    public T Data { get; set; }
    public string Error { get; set; }
}