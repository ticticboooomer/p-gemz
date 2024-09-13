using Gemz.Api.Collector.Service.Collector.Model;

namespace Gemz.Api.Collector.Service.Collector
{
    public interface IDashboardService
    {
        Task<GenericResponse<TotalGemsOutputModel>> GetTotalGemsPurchased(string collectorId);

        Task<GenericResponse<TotalGemsOpenedOutputModel>> GetTotalGemsOpened(string collectorId);
        Task<GenericResponse<TotalGemsUnopenedOutputModel>> GetTotalGemsUnopened(string collectorId);

        Task<GenericResponse<TotalGemsByCreatorOutputModel>> GetTotalGemsByCreator(string collectorId);
    }
}
