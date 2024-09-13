using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Collector.Service.Collector.Model
{
    public class PurchCollectionGemsOutputModel
    {
        public string GemId { get; set; }
        public string GemName { get; set; }
        public int Rarity { get; set; }
        public string ImageId { get; set; }
        public int SizePercentage { get; set; }
        public int PositionXPercentage { get; set; }
        public int PositionYPercentage { get; set; }
        public int Quantity { get; set; }
    }
}
