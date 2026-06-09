public class Content
{
    // showid is the primary key for the content entity. it is a unique identifier for each piece of content (movie or TV show) in the database. it is used to link the content to its genres and to track user interactions with the content.
    public string ShowId { get; set; } = "";
    public string Title { get; set; } = "";
    public string ContentType { get; set; } = "Movie";
    public int ClusterId { get; set; }
}