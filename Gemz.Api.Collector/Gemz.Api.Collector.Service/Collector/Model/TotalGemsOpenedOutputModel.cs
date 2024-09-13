using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Collector.Service.Collector.Model
{
    public class TotalGemsOpenedOutputModel
    {
        public int TotalOpenedGems { get; set; }
        public int TotalGemsRevealed { get; set; }
        public int TotalGemsForCreatorOpening { get; set; }
    }
}
