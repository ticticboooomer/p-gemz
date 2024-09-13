using Gemz.Api.Collector.Data.Model;

namespace Gemz.Api.Collector.Data.Repository;

public interface IGemRepository
{
    Task<List<Gem>> FetchAllGemsForCollection(string collectionId);

    Task<GemsPage> GetGemsPageByCollectionId(string collectionId, int currentPage, int pageSize);

    Task<Gem> FetchSingleGem(string gemId);

    Task<Gem> FetchSingleGemAnyStatus(string gemId);
}