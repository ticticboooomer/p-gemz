using Gemz.Api.Creator.Data.Model;

namespace Gemz.Api.Creator.Data.Repository;

public interface IGemRepository
{
    Task<Gem> GetAsync(string gemId);

    Task<Gem> CreateAsync(Gem entity);

    Task<GemsPage> GetGemsPageByCollectionId(string collectionId, string creatorId, int currentPage, int pageSize);
    Task<bool> PatchItemPublishedStatusAsync(string gemId, int publishedStatus);

    Task<Gem> UpdateGemAsync(Gem entity);
    Task<bool> PatchGemDeletedAsync(string gemId);
    Task<bool> AnyPublishedGemsInCollection(string collectionId);
}