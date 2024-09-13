namespace Gemz.Api.Creator.Data.Model
{
    public class CreatorToOpen : BaseDataModel
    {
        public string CreatorId { get; set; }
        public string CollectorId { get; set; }
        public string CollectorGemsId { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
