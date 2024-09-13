using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemz.Api.Creator.Data.Model;

namespace Gemz.Api.Creator.Data.Repository;
public interface IOverlayKeyRepository
{
    Task<List<OverlayKey>> GetAllForCreatorAsync(string creatorId);
    Task<int> CountKeysAsync(string creatorId);
    Task<OverlayKey> CreateKeyAsync(OverlayKey model);
    Task<string> RevokeKey(string creatorId, string keyId);
    Task<OverlayKey> GetKeyByContent(string keyContent);
}
