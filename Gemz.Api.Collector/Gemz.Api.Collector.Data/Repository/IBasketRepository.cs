using Gemz.Api.Collector.Data.Model;

namespace Gemz.Api.Collector.Data.Repository;

public interface IBasketRepository
{
    Task<Basket> GetCreatorBasketForCollector(string collectorId, string creatorId);
    Task<Basket> GetActiveBasketById(string basketId);
    Task<Basket> UpdateBasketAsync(Basket entity);
    Task<Basket> CreateBasketAsync(Basket entity);
    Task<bool> DeactivateBasket(string basketId);
    Task<List<Basket>> GetAllBasketsForCollector(string collectorId);

    Task<BasketsPage> GetPageOfBasketsForCollector(string collectorId, int currentPage, int pageSize,
        string excludeCreator);
    Task<Basket> GetAnyStatusBasketById(string basketId);

}