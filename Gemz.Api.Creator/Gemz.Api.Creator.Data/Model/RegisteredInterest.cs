namespace Gemz.Api.Creator.Data.Model
{
    public class RegisteredInterest : BaseDataModel
    {
        public string AccountId { get; set; }
        public string RequestMessage { get; set; }
        public int ApprovalStatus { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
