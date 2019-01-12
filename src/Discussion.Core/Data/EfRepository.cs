using System;
using System.Linq;
using Discussion.Core.Models;
using Discussion.Core.Time;
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
        private readonly IClock _clock;
        private readonly IReadonlyDataSettings _readonlyDataSettings;

        public EfRepository(ApplicationDbContext context, IClock clock, IReadonlyDataSettings readonlyDataSettings)
        {
            this._context = context;
            _clock = clock;
            _readonlyDataSettings = readonlyDataSettings;
            
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
            if (_readonlyDataSettings.IsReadonly)
            {
                return;
            }
            
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (entity.CreatedAtUtc == Entity.EntityInitialDate)
            {
                entity.CreatedAtUtc = _clock.Now.UtcDateTime;    
            }

            _entities.Add(entity);
            _context.SaveChanges();
        }

        public void Update(T entity)
        {
            if (_readonlyDataSettings.IsReadonly)
            {
                return;
            }
            
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            if (entity.ModifiedAtUtc == Entity.EntityInitialDate)
            {
                entity.ModifiedAtUtc = _clock.Now.UtcDateTime;    
            }
            
            _context.SaveChanges();
        }

        public void Delete(T entity)
        {
            if (_readonlyDataSettings.IsReadonly)
            {
                return;
            }
            
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            _entities.Remove(entity);
            _context.SaveChanges();
        }

    }
}