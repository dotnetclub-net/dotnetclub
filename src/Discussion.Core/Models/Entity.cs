using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Discussion.Core.Models
{
    public abstract class Entity
    {
        public static readonly DateTime EntityInitialDate = new DateTime(2002, 2, 13, 0, 0, 0, DateTimeKind.Utc);  
        
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public DateTime CreatedAtUtc { get; set; } = EntityInitialDate;

        public DateTime ModifiedAtUtc { get; set; } = EntityInitialDate;
    }
}
