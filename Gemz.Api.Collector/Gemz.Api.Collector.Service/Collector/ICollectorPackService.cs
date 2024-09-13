using Gemz.Api.Collector.Service.Collector.Model;

namespace Gemz.Api.Collector.Service.Collector
{
    public interface ICollectorPackService
    {
        Task<GenericResponse<List<UnopenedPackOutputModel>>> FetchUnopenedPacksForCollector(string collectorId);

        Task<GenericResponse<OpenPacksOutputModel>> OpenCollectorPacksInCollection(string collectorId, OpenPacksInputModel openPacksInputModel);
        Task<GenericResponse<OpenPackSessionOutputModel>> CheckOpenPackSession(string collectorId, OpenPacksSessionInputModel openPacksSessionInputModel);
    }
}
