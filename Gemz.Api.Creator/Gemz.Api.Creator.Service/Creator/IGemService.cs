using Gemz.Api.Creator.Service.Creator.Model;

namespace Gemz.Api.Creator.Service.Creator;

public interface IGemService
{
    Task<GenericResponse<FetchGemOutputModel>> FetchGemById(GemIdModel gemIdModel, string creatorId);
    Task<GenericResponse<CreateGemOutputModel>> CreateGem(GemModel gemModel, string creatorId);
    Task<GenericResponse<GemCollectionModel>> FetchGemsInCollection(GemsPagingModel gemsPagingModel, string creatorId);
    Task<GenericResponse<UpdateStatusGemOutputModel>> UpdatePublishedStatusForGem(GemIdModel gemIdModel, string creatorId, int publishedStatus);
    Task<GenericResponse<UpdateGemOutputModel>> UpdateGem(GemUpdateModel gemUpdateModel, string creatorId);
    Task<GenericResponse<ArchiveGemOutputModel>> ArchiveGem(GemIdModel gemIdModel, string creatorId);
}