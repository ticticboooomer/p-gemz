using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemz.Api.Collector.Data.Model;

namespace Gemz.Api.Collector.Data.Repository;
public interface IAccountRepository
{
    Task<Account> GetAccountById(string id);
}
