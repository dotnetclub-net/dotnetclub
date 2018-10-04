using System.Linq;

namespace Discussion.Core.Data
{
    public interface IRepository<T>
    {
        void Save(T item); 
        IQueryable<T> All(); 
        T Get(int id); 
        void Delete(T item); 
        void Update(T item);
    }
}