using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;

namespace carto.Models
{
    public class RepositoryAdapterMongoDb<T>:IRepositoryAdapter<T>
    {
        private readonly MongoDatabase _database;
        private readonly MongoCollection<T> _dbCollection;
        private readonly Expression<Func<T, long>> _idGetter;

        public RepositoryAdapterMongoDb(string collectionName)
        {
            var param = Expression.Parameter(typeof(T));
            var member = Expression.Property(param, "Id");
            _idGetter = Expression.Lambda<Func<T, long>>(member, param);
            var connectionString = ConfigurationManager.ConnectionStrings["MongoDb"].ConnectionString;
            var client = new MongoClient(connectionString);
            _database = client.GetServer().GetDatabase("carto");
            _dbCollection = _database.GetCollection<T>(collectionName);
        }

        public IEnumerable<T> ReadAll()
        {
            return _dbCollection.FindAll();
        }

        public IEnumerable<T> ReadAll(long graphId)
        {
            var query = Query.EQ("GraphId", graphId);
            return _dbCollection.Find(query);
        }

        public T Create(T item)
        {
            _dbCollection.Insert(item);
            return item;
        }

        public T Update(T item)
        {
            _dbCollection.Save(item);
            return item;
        }

        public bool Delete(long id)
        {
            var query = Query.EQ("_id", id);
            var res = _dbCollection.Remove(query);
            return res.Ok;
        }

        public long ReadMaxId()
        {
            return _dbCollection.AsQueryable<T>().Select(_idGetter).Max();
        }

        public T Read(long id)
        {
            var query = Query.EQ("_id", id);
            return _dbCollection.FindOne(query);
        }
    }
}