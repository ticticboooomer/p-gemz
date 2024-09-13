using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Collector.Data.Model
{
    public class CreatorToOpen : BaseDataModel
    {
        public string CreatorId { get; set; }
        public string CollectorId { get; set; }
        public string CollectorGemsId { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
