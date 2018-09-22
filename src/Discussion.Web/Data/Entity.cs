using System;
using System.ComponentModel.DataAnnotations;

namespace Discussion.Web.Data
{
    public abstract class Entity 
    {
        [Key]
        public int Id { get; set; }
        
        public DateTime CreatedAtUtc { get; set; }        
        
        public DateTime ModifiedAtUtc { get; set; }
    }
}
