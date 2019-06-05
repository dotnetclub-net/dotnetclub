using Discussion.Core.Models;
using Discussion.Tools.MigrationToIds4.Models;
using Microsoft.EntityFrameworkCore;

namespace Discussion.Tools.MigrationToIds4
{
    public class IdsContext : DbContext
    {
        public DbSet<IdentityUsers> IdentityUsers { get; set; }
        
        public IdsContext(DbContextOptions<IdsContext> options):base(options)
        {
            IdentityUsers = Set<IdentityUsers>();
        }
    }
}