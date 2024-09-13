using Gemz.Api.Collector.Service.Collector.Model;

namespace Gemz.Api.Collector.Service.Collector;

public interface IGemService
{
    Task<GenericResponse<GemSampleWithCountModel>> FetchFixedQuantityOfGems(GemFixedQtyInputModel gemFixedQtyInputModel);
    Task<GenericResponse<GemsPagedModel>> GetPagedGemsForCollection(GemPagingModel gemPagingModel);
    Task<GenericResponse<SingleGemOutputModel>> GetSingleGemById(SingleGemInputModel singleGemInputModel);
}