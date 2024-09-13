using Gemz.Api.Collector.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Collector.Service.Collector.Model
{
    public class GemsByRarity
    {
        public List<Gem> CommonGems { get; set; }
        public List<Gem> RareGems { get; set; }
        public List<Gem> EpicGems { get; set; }
        public List<Gem> LegendaryGems { get; set; }
        public List<Gem> MythicGems { get; set; }
    }
}
