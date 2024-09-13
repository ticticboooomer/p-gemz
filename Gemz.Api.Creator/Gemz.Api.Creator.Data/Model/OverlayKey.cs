using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Creator.Data.Model;
public class OverlayKey : BaseDataModel
{
    public string CreatorId { get; set; }
    public string KeyContent { get; set; }
    public DateTime CreatedAt { get; set; }
}
