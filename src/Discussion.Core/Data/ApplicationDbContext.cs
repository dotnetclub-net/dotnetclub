using System.Linq;
using Discussion.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Discussion.Core.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var entityType = typeof(Entity);
            entityType
                .Assembly
                .GetExportedTypes()
                .Where(type => !type.IsAbstract && !type.IsInterface && entityType.IsAssignableFrom(type))
                .ToList()
                .ForEach(etype => modelBuilder.Entity(etype));
        }
    }
}