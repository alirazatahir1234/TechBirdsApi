namespace TechBirdsWebAPI.Models
{
    public class Article
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int AuthorId { get; set; } = 0;
        public int CategoryId { get; set; } = 0;
        public DateTime PublishedAt { get; set; }
    }

   
}