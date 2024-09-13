using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemz.Api.Creator.Service.Creator.Model;

namespace Gemz.Api.Creator.Service.Creator
{
    public interface IInterestService
    {
        Task<GenericResponse<RegisterInterestOutputModel>> RegisterInterest(string accountId, RegisterInterestInputModel registerInterestInputModel);
    }
}
