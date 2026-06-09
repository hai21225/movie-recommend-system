public class UserInteractions
{
    // primarykey = InteractionId
    public int InteractionId { get; set; }
    public int UserId { get; set; }
    public string ShowId { get; set; } = "";
    public bool Liked { get; set; }
    public DateTime Timestamp { get; set; }
}