using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System;
using System.Linq;

namespace carto.Models
{
    public class RepositoryAdapterStub<T> : IRepositoryAdapter<T>
    {
        private readonly ConcurrentDictionary<long, T> _dico;
        private readonly Func<T, long> _idGetter;

        public RepositoryAdapterStub(Func<T, long> idGetter, IEnumerable<T> seed)
        {
            _idGetter = idGetter;
            _dico = new ConcurrentDictionary<long, T>(seed.ToDictionary(x => _idGetter(x)));
        }

        public IEnumerable<T> ReadAll()
        {
            return _dico.Values;
        }

        public IEnumerable<T> ReadAll(long graphId)
        {
            return ReadAll();
        }

        public T Create(T item)
        {
            _dico.TryAdd(_idGetter(item), item);
            return item;
        }

        public T Update(T item)
        {
            _dico[_idGetter(item)]= item;
            return item;
        }

        public bool Delete(long id)
        {
            T item;
            return _dico.TryRemove(id, out item);
        }

        public long ReadMaxId()
        {
            if (_dico.Any())
            {
                return _dico.Keys.Max();
            }
            else {
                return 0;
            }
        }

        public T Read(long id)
        {
            return _dico[id];
        }
    }
}