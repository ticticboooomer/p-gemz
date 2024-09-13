using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Creator.Service.Creator.Model;

public class OverlayKeyModel
{
    public string Id { get; set; }
    public string CreatorId { get; set; }
    public string KeyContent { get; set; }
    public DateTime CreatedAt { get; set; }
}