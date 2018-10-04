using System;
using System.Linq;
using Discussion.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Discussion.Core.Data
{
   
    /// <summary>
    /// Default repository pattern implementation based on EF DBSet.
    /// </summary>
    /// <remarks>
    /// Please refer to following links to see original materials:
    /// https://code.msdn.microsoft.com/Generic-Repository-Pattern-f133bca4
    /// https://social.technet.microsoft.com/wiki/contents/articles/36287.repository-pattern-in-asp-net-core.aspx
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class EfRepository<T> : IRepository<T> where T: Entity
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _entities;

        public EfRepository(ApplicationDbContext context)
        {
            this._context = context;
            _entities = context.Set<T>();
        }
        
        public IQueryable<T> All()
        {
            return _entities;
        }

        public T Get(int id)
        {
            return _entities.Find(id);
        }

        public void Save(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (entity.CreatedAtUtc == Entity.EntityInitialDate)
            {
                entity.CreatedAtUtc = DateTime.UtcNow;    
            }

            _entities.Add(entity);
            _context.SaveChanges();
        }

        public void Update(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            if (entity.ModifiedAtUtc == Entity.EntityInitialDate)
            {
                entity.ModifiedAtUtc = DateTime.UtcNow;    
            }
            
            _context.SaveChanges();
        }

        public void Delete(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            _entities.Remove(entity);
            _context.SaveChanges();
        }

    }
}