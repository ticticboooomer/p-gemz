using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Creator.Data.Model
{
    public class CollectorGem : BaseDataModel
    {
        public string CollectorId { get; set; }
        public string CreatorId { get; set; }
        public string CollectionId { get; set; }
        public string GemId { get; set; }
        public bool Visible { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
