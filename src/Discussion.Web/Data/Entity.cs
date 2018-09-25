using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Discussion.Web.Data
{
    public abstract class Entity
    {
        public static readonly DateTime EntityInitialDate = new DateTime(2002, 2, 13);  
        
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public DateTime CreatedAtUtc { get; set; } = EntityInitialDate;

        public DateTime ModifiedAtUtc { get; set; } = EntityInitialDate;
    }
}
