namespace PokerAPIMPwDB.DTO.Actions
{
    public class PlayerActionDto
{
    public Guid PlayerId {get;set;}
    public string PlayerName { get; set; }
    public int Amount { get; set; } // untuk bet / raise
}
    
}