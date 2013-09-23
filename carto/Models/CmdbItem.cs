using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace carto.Models
{
    public class CmdbItem
    {
        public CmdbItem(CmdbItemCategory category, long id, string name)
        {
            Attributes = new Dictionary<long, object>();
            Category = category;
            Id = id;
            Name = name;
        }

        public CmdbItemCategory Category { get; private set; }

        public long Id { get; private set; }
        public long Version { get; set; }

        [Required]
        public string Name { get; private set; }
        public string Description { get; set; }

        public IDictionary<long,object> Attributes { get; set; }
    }
}