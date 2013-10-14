using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using carto.Models;

namespace carto.Api
{
    public class GraphController : ApiController
    {
        [HttpGet]
        [ActionName("graph")]
        public IEnumerable<CmdbGraph<CmdbItem, CmdbDependency>> GetGraph()
        {
            return CmdbRepository.Instance.ReadAll();
        }

        [ActionName("node")]
        public IQueryable<VertexDto<CmdbItem, CmdbDependency>> Get(long graphId)
        {
            var graph = CmdbRepository.Instance.ReadGraph(graphId);
            return graph.Vertices.Select(v => new VertexDto<CmdbItem, CmdbDependency>(v, graph.OutEdges(v))).AsQueryable();
        }

        [ActionName("node")]
        public CmdbItem Put(long id, CmdbItem item)
        {
            return CmdbRepository.Instance.Update(item);
        }


        [ActionName("node")]
        public CmdbItem Post(CmdbItem item)
        {
            return CmdbRepository.Instance.Create(item);
        }

        [ActionName("node")]
        public bool Delete(long id)
        {
            return CmdbRepository.Instance.Delete(id);
        }

        [HttpPost]
        [ActionName("link")]
        public CmdbDependency PostLink(CmdbDependency edge)
        {
            return CmdbRepository.Instance.AddEdge(edge);
        }

        [HttpGet]
        [ActionName("link")]
        public CmdbDependency GetLink(long id)
        {
            return CmdbRepository.Instance.Graph.Edges.First(e => e.Id == id);
        }

        [HttpDelete]
        [ActionName("link")]
        public bool DeleteLink(long id)
        {
            return CmdbRepository.Instance.DeleteLink(id);
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
