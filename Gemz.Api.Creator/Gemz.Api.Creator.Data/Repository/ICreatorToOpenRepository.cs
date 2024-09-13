using Gemz.Api.Creator.Data.Model;

namespace Gemz.Api.Creator.Data.Repository
{
    public interface ICreatorToOpenRepository
    {
        Task<List<CreatorToOpen>> GetAllForOneCreator(string creatorId);
        Task<CreatorToOpen> GetById(string id);
        Task<bool> DeleteSingle(string id);
    }
}
