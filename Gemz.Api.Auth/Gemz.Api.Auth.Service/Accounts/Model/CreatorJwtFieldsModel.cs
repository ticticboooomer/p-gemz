using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Auth.Service.Accounts.Model
{
    public class CreatorJwtFieldsModel
    {
        public bool ValidData { get; set; }
        public bool IsCreator { get; set; }
        public int OnboardingStatus { get; set; }
        public int RestrictedStatus { get; set; }
    }
}
