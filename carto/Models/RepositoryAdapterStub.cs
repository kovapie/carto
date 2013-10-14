using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace carto.Models
{
    public class RepositoryAdapterStub<T>:IRepositoryAdapter<T>
    {
        public IEnumerable<T> ReadAll()
        {
            return new Collection<T>();
        }

        public IEnumerable<T> ReadAll(long graphId)
        {
            return new Collection<T>();
        }

        public T Create(T item)
        {
            return item;
        }

        public T Update(T item)
        {
            return item;
        }

        public bool Delete(long id)
        {
            return true;
        }

        public long ReadMaxId()
        {
            return 1;
        }

        public T Read(long id)
        {
            return default(T);
        }
    }
}