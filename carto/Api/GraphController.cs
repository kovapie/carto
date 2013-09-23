using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using Newtonsoft.Json;
using QuickGraph;
using QuickGraph.Graphviz;
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
