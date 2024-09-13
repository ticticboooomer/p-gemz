using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Creator.Service.Creator.Model
{
    public class FetchGemOutputModel
    {
        public string Id { get; set; }
        public string CollectionId { get; set; }
        public string CreatorId { get; set; }
        public string Name { get; set; }
        public int Rarity { get; set; }
        public string ImageId { get; set; }
        public int SizePercentage { get; set; }
        public int PositionXPercentage { get; set; }
        public int PositionYPercentage { get; set; }
        public int PublishedStatus { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
