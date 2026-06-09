public class UserInteractionsDto
{
    public int InteractionId { get; set; }
    public string ShowId { get; set; } = "";
    public int UserId { get; set; } 
    public bool IsLiked { get; set; }
    public DateTime InteractedAt { get; set; }
}