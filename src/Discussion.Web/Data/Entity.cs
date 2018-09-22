using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Discussion.Web.Data
{
    public abstract class Entity 
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public DateTime CreatedAtUtc { get; set; }        
        
        public DateTime ModifiedAtUtc { get; set; }
    }
}
