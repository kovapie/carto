using System.Collections.Generic;

namespace carto.Models
{
    public interface IRepositoryAdapter<T>
    {
        IEnumerable<T> ReadAll();
        IEnumerable<T> ReadAll(long graphId);
        T Create(T item);
        T Update(T item);
        bool Delete(long id);
        long ReadMaxId();
        T Read(long id);
    }
}