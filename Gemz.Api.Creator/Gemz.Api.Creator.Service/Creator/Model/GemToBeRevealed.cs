using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Creator.Service.Creator.Model
{
    public class GemToBeRevealed
    {
        public string CreatorToOpenId { get; set; }
        public string CollectorName { get; set; }
        public string CollectorGemsId { get; set; }
        public string CollectionName { get; set; }
        public string WaitingSinceDate { get; set; }
        public string WaitingSinceTime { get; set; }
    }
}
