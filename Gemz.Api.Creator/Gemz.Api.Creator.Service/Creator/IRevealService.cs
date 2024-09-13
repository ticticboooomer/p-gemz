using Gemz.Api.Creator.Service.Creator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Creator.Service.Creator
{
    public interface IRevealService
    {
        Task<GenericResponse<GemsToBeRevealedOutputModel>> GetGemsTobeRevealedByCreator(string creatorId);

        Task<GenericResponse<SingleGemRevealOutputModel>> RevealSingleGem(string creatorId, SingleGemRevealInputModel singleGemRevealInputModel);

    }
}
