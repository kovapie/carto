using System;
using System.Collections.Generic;

namespace carto.Models
{
    public class CmdbAttributeDefinition
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public Type Type { get; set; }
        public ICollection<object> AuthorisedValues { get; set; }
    }
}