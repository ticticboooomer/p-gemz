
namespace Gemz.Api.Collector.Data.Model
{
    public class OpenPackSessionGem
    {
        public string GemId { get; set; }
        public string Name { get; set; }
        public int Rarity { get; set; }
        public string ImageId { get; set; }
        public int SizePercentage { get; set; }
        public int PositionXPercentage { get; set; }
        public int PositionYPercentage { get; set; }
        public int NumberOpened { get; set; }
    }
}
