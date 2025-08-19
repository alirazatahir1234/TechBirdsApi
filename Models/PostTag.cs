namespace TechBirdsWebAPI.Models
{
    public class PostTag
    {
        public int PostId { get; set; }
        public int TagId { get; set; }
        
        // Navigation Properties
        public virtual Post? Post { get; set; }
        public virtual Tag? Tag { get; set; }
    }
}
