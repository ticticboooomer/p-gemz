using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.ServiceBus.Model;
public class NotifyOrderModel
{
    public string? CreatorId { get; set; }
    public string? CollectorName { get; set; }
    public int? Packs { get; set; }
}
