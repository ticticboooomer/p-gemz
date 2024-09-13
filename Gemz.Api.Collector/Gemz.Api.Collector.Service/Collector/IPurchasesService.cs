using Gemz.Api.Collector.Service.Collector.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Collector.Service.Collector
{
    public interface IPurchasesService
    {
        Task<GenericResponse<List<StoresPurchasedFromOutputModel>>> FetchStoresContainingPurchases(string collectorId);
        Task<GenericResponse<PurchAllCollectionsOutputModel>> FetchCollectionsOneStoreContainingPurchases(string collectorId, PurchCollectionsInputModel purchCollectionsInputModel);
        Task<GenericResponse<PurchAllGemsCollectionOutputModel>> FetchAllPurchasedGemsInOneCollection(string collectorId, PurchAllGemsCollectionInputModel purchAllGemsCollectionInputModel);
    }
}
