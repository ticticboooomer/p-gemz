using Gemz.Api.Collector.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Collector.Data.Repository
{
    public interface ICreatorToOpenRepository
    {
        Task<CreatorToOpen> CreateAsync(CreatorToOpen entity);
    }
}
