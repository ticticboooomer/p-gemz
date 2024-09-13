using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Collector.Service.Collector.Model
{
    public class TotGemsPurchByCreatorOutputModel
    {
        public string CreatorStoreTag { get; set; }
        public string CreatorStoreName { get; set; }
        public string CreatorLogoImageId { get; set; }
        public int TotalGemsPurchased { get; set; }
        public int TotalGemsOpened { get; set; }
        public int TotalGemsWaitingForCreatorReveal { get; set; }
        public int TotalPacksUnopened { get; set; }
        public int TotalGemsUnopened { get; set; }
    }
}
