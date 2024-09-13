using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Collector.Service.Collector.Model
{
    public class LiveStoresPagedOutputModel
    {
        public List<LiveStoresOutputModel> Stores { get; set; }
        public int ThisPage { get; set; }
        public int TotalPages { get; set; }
    }
}
