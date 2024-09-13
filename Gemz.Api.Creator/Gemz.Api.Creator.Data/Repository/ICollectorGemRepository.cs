using Gemz.Api.Creator.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Creator.Data.Repository
{
    public interface ICollectorGemRepository
    {
        Task<CollectorGem> GetFirstForCollection(string creatorId, string collectionId);

        Task<CollectorGem> GetFirstForGem(string creatorId, string gemId);

        Task<CollectorGem> GetSingleCollectorGem(string collectorGemId);

        Task<CollectorGem> UpdateSingle(CollectorGem entity);
    }
}
