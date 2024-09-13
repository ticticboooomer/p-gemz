using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Collector.Data.Model;
public class Account : BaseDataModel
{
    public string TwitchUserId { get; set; }
    public string TwitchEmail { get; set; }
    public string EmailAddress { get; set; }
    public bool TwitchEmailVerified { get; set; }
    public bool IsCreator { get; set; }
    public string TwitchUsername { get; set; }
    public DateTime CreatedOn { get; set; }
    public TwitchTokens Tokens { get; set; }

    public class TwitchTokens
    {
        public string AccessCode { get; set; }
        public string RefreshCode { get; set; }
    }
    public int OnboardingStatus { get; set; }
    public string StripeAccountId { get; set; }
    public bool StripeDetailsSubmitted { get; set; }
    public bool StripeChargesEnabled { get; set; }
    public int RestrictedStatus { get; set; }
    public int CommissionPercentage { get; set; }

}
