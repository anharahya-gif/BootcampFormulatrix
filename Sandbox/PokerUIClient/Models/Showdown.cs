namespace PokerUIClient.Models
{

    public class ShowdownDTO
    {
        public List<string> Winners { get; set; } = new();
        public string? HandRank { get; set; }
        public string? Message { get; set; }
    }
}