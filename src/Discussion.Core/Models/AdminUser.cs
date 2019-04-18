namespace Discussion.Core.Models
{
    public class AdminUser: Entity
    {
        public string Username { get; set; }
        
        public string HashedPassword { get; set; }
        
    }
}