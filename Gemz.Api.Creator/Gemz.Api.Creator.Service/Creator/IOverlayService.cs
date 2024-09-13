using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemz.Api.Creator.Service.Creator.Model;

namespace Gemz.Api.Creator.Service.Creator;
public interface IOverlayService
{
    Task<GenericResponse<OverlayTestResponseModel>> SendOverlayTestOrder(string creatorId);
    Task<GenericResponse<OverlayKeyModel>> CreateOverlayKey(string creatorId);
    Task<GenericResponse<List<OverlayKeyModel>>> GetOverlayKeysForCreator(string creatorId);
    Task<GenericResponse<string>> RevokeKey(string creatorId, string keyId);
    Task<ValidatedOverlayKeyModel> ValidateKey(string key);
}
