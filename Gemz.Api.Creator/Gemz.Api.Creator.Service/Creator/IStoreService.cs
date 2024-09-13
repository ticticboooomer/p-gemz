using Gemz.Api.Creator.Data.Model;
using Gemz.Api.Creator.Service.Creator.Model;

namespace Gemz.Api.Creator.Service.Creator;

public interface IStoreService
{
    Task<GenericResponse<StoreModel>> EditOrInsertStoreDetails(StoreUpsertModel storeUpsertModel, string creatorId);

    Task<GenericResponse<StoreModel>> FetchCreatorStoreDetails(string creatorId);

    Task<GenericResponse<TagWordValidityModel>> CheckTagWordAvailable(string creatorId, TagWordModel tagWordModel);
}