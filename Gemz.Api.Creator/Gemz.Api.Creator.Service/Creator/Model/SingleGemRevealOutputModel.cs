namespace Gemz.Api.Creator.Service.Creator.Model
{
    public class SingleGemRevealOutputModel
    {
        public string CollectorName { get; set; }
        public string GemId { get; set; }
        public string CollectionName { get; set; }
        public string GemName { get; set; }
        public int Rarity { get; set; }
        public string ImageId { get; set; }
        public int SizePercentage { get; set; }
        public int PositionXPercentage { get; set; }
        public int PositionYPercentage { get; set; }
        public int PublishedStatus { get; set; }
    }
}
