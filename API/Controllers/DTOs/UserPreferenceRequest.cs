public class UserPreferenceRequest
{
    public int UserId { get; set; }
    public List<int> GenreIds { get; set; } = new List<int>();
}