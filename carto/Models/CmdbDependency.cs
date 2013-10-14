using System.Dynamic;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace carto.Models
{
    public class CmdbDependency : QuickGraph.IEdge<CmdbItem>
    {
        public CmdbDependency(long id, CmdbItem source, CmdbItem target)
        {
            Attributes = new ExpandoObject();
            Id = id;
            SourceId = source.Id;
            TargetId = target.Id;
            Source = source;
            Target = target;
        }

        public long SourceId { get; set; }
        public long TargetId { get; set; }

        public long Id { get; set; }
        public long Version { get; set; }
        public long GraphId { get; set; }

        public dynamic Attributes { get; set; }

        //public string Transport { get; set; }
        //public string DataType { get; set; }
        [JsonIgnore]
        [BsonIgnore]
        public CmdbItem Source { get; set; }

        [BsonIgnore]
        public CmdbItem Target { get; set; }
    }
}