using Gemz.Api.Creator.Service.Creator.Model;

namespace Gemz.Api.Creator.Service.Creator
{
    public interface IDashboardService
    {
        Task<GenericResponse<CollectionStatsOutputModel>> GetCollectionStats(string creatorId);

        Task<GenericResponse<GemsToBeOpenedStatsOutputModel>> GetGemsToBeOpenedStats(string creatorId);
    }
}
