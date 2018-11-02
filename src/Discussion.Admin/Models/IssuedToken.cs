namespace Discussion.Admin.Models
{
    public class IssuedToken
    {
        public string TokenString { get; set; }
        public int ValidForSeconds { get; set; }
    }
}