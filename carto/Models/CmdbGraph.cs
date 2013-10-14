using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using QuickGraph;

namespace carto.Models
{
    public class CmdbGraph<TVertex, TEdge> : IEdgeListAndIncidenceGraph<TVertex, TEdge>, IMutableBidirectionalGraph<TVertex, TEdge> where TEdge : IEdge<TVertex>
    {
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }
        [JsonProperty(PropertyName = "version")]
        public long Version { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        private readonly BidirectionalGraph<TVertex, TEdge> _graph = new BidirectionalGraph<TVertex, TEdge>();

        public bool ContainsVertex(TVertex v)
        {
            return _graph.ContainsVertex(v);
        }

        public bool IsOutEdgesEmpty(TVertex v)
        {
            return _graph.IsOutEdgesEmpty(v);
        }

        public int OutDegree(TVertex v)
        {
            return _graph.OutDegree(v);
        }

        public IEnumerable<TEdge> OutEdges(TVertex v)
        {
            return _graph.OutEdges(v);
        }

        public bool TryGetInEdges(TVertex v, out IEnumerable<TEdge> edges)
        {
            return _graph.TryGetInEdges(v, out edges);
        }

        public bool TryGetOutEdges(TVertex v, out IEnumerable<TEdge> edges)
        {
            return _graph.TryGetOutEdges(v, out edges);
        }

        public TEdge OutEdge(TVertex v, int index)
        {
            return _graph.OutEdge(v, index);
        }

        public bool IsInEdgesEmpty(TVertex v)
        {
            return _graph.IsInEdgesEmpty(v);
        }

        public int InDegree(TVertex v)
        {
            return _graph.InDegree(v);
        }

        public IEnumerable<TEdge> InEdges(TVertex v)
        {
            return _graph.InEdges(v);
        }

        public TEdge InEdge(TVertex v, int index)
        {
            return _graph.InEdge(v, index);
        }

        public int Degree(TVertex v)
        {
            return _graph.Degree(v);
        }

        public bool ContainsEdge(TVertex source, TVertex target)
        {
            return _graph.ContainsEdge(source, target);
        }

        public bool TryGetEdge(TVertex source, TVertex target, out TEdge edge)
        {
            return _graph.TryGetEdge(source, target, out edge);
        }

        public bool TryGetEdges(TVertex source, TVertex target, out IEnumerable<TEdge> edges)
        {
            return _graph.TryGetEdges(source, target, out edges);
        }

        public bool ContainsEdge(TEdge edge)
        {
            return _graph.ContainsEdge(edge);
        }

        public bool AddVertex(TVertex v)
        {
            return _graph.AddVertex(v);
        }

        public int AddVertexRange(IEnumerable<TVertex> vertices)
        {
            return _graph.AddVertexRange(vertices);
        }

        public bool RemoveVertex(TVertex v)
        {
            return _graph.RemoveVertex(v);
        }

        public int RemoveVertexIf(VertexPredicate<TVertex> predicate)
        {
            return _graph.RemoveVertexIf(predicate);
        }

        public bool AddEdge(TEdge e)
        {
            return _graph.AddEdge(e);
        }

        public int AddEdgeRange(IEnumerable<TEdge> edges)
        {
            return _graph.AddEdgeRange(edges);
        }

        public bool AddVerticesAndEdge(TEdge e)
        {
            return _graph.AddVerticesAndEdge(e);
        }

        public int AddVerticesAndEdgeRange(IEnumerable<TEdge> edges)
        {
            return _graph.AddVerticesAndEdgeRange(edges);
        }

        public bool RemoveEdge(TEdge e)
        {
            return _graph.RemoveEdge(e);
        }

        public int RemoveEdgeIf(EdgePredicate<TVertex, TEdge> predicate)
        {
            return _graph.RemoveEdgeIf(predicate);
        }

        public int RemoveOutEdgeIf(TVertex v, EdgePredicate<TVertex, TEdge> predicate)
        {
            return _graph.RemoveOutEdgeIf(v, predicate);
        }

        public int RemoveInEdgeIf(TVertex v, EdgePredicate<TVertex, TEdge> predicate)
        {
            return _graph.RemoveInEdgeIf(v, predicate);
        }

        public void ClearOutEdges(TVertex v)
        {
            _graph.ClearOutEdges(v);
        }

        public void ClearInEdges(TVertex v)
        {
            _graph.ClearInEdges(v);
        }

        public void ClearEdges(TVertex v)
        {
            _graph.ClearEdges(v);
        }

        public void TrimEdgeExcess()
        {
            _graph.TrimEdgeExcess();
        }

        public void Clear()
        {
            _graph.Clear();
        }

        public void MergeVertex(TVertex v, EdgeFactory<TVertex, TEdge> edgeFactory)
        {
            _graph.MergeVertex(v, edgeFactory);
        }

        public void MergeVertexIf(VertexPredicate<TVertex> vertexPredicate, EdgeFactory<TVertex, TEdge> edgeFactory)
        {
            _graph.MergeVertexIf(vertexPredicate, edgeFactory);
        }

        public BidirectionalGraph<TVertex, TEdge> Clone()
        {
            return _graph.Clone();
        }

        [BsonIgnore]
        public int EdgeCapacity
        {
            get { return _graph.EdgeCapacity; }
            set { _graph.EdgeCapacity = value; }
        }

        public bool IsDirected
        {
            get { return _graph.IsDirected; }
        }

        public bool AllowParallelEdges
        {
            get { return _graph.AllowParallelEdges; }
        }

        public bool IsVerticesEmpty
        {
            get { return _graph.IsVerticesEmpty; }
        }

        public int VertexCount
        {
            get { return _graph.VertexCount; }
        }

        public IEnumerable<TVertex> Vertices
        {
            get { return _graph.Vertices; }
        }

        public bool IsEdgesEmpty
        {
            get { return _graph.IsEdgesEmpty; }
        }

        public int EdgeCount
        {
            get { return _graph.EdgeCount; }
        }

        public IEnumerable<TEdge> Edges
        {
            get { return _graph.Edges; }
        }

        public event VertexAction<TVertex> VertexAdded
        {
            add { _graph.VertexAdded += value; }
            remove { _graph.VertexAdded -= value; }
        }

        public event VertexAction<TVertex> VertexRemoved
        {
            add { _graph.VertexRemoved += value; }
            remove { _graph.VertexRemoved -= value; }
        }

        public event EdgeAction<TVertex, TEdge> EdgeAdded
        {
            add { _graph.EdgeAdded += value; }
            remove { _graph.EdgeAdded -= value; }
        }

        public event EdgeAction<TVertex, TEdge> EdgeRemoved
        {
            add { _graph.EdgeRemoved += value; }
            remove { _graph.EdgeRemoved -= value; }
        }

        public event EventHandler Cleared
        {
            add { _graph.Cleared += value; }
            remove { _graph.Cleared -= value; }
        }
   }
}