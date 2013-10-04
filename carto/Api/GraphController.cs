using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using carto.Models;

namespace carto.Api
{
    public class GraphController : ApiController
    {
        public IQueryable<VertexDto<CmdbItem, CmdbDependency>> Get()
        {
            var graph = CmdbRepository.Instance.Graph;
            return graph.Vertices.Select(v => new VertexDto<CmdbItem, CmdbDependency>(v, graph.OutEdges(v))).AsQueryable();
        }

        public CmdbItem Put(long id, CmdbItem item)
        {
            return CmdbRepository.Instance.Update(item);
        }


        public CmdbItem Post(CmdbItem item)
        {
            return CmdbRepository.Instance.Create(item);
        }

        public bool Delete(long id)
        {
            return CmdbRepository.Instance.Delete(id);
        }
    }

    public class VertexDto<TVertex, TEdge>
    {
        [JsonProperty(PropertyName = "vertex")]
        public TVertex Vertex { get; set; }
        [JsonProperty(PropertyName = "edges")]
        public IEnumerable<TEdge> Edges { get; set; }

        public VertexDto(TVertex v, IEnumerable<TEdge> e)
        {
            Vertex = v;
            Edges = e;
        }
    }
}
