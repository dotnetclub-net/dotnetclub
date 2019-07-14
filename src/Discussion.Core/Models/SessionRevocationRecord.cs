
namespace Discussion.Core.Models
{
    public class SessionRevocationRecord : Entity
    {
        public string SessionId { get; set; }
        
        public string Reason { get; set; }
    }
}