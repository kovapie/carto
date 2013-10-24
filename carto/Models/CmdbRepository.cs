using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using MongoDB.Bson;

namespace carto.Models
{
    public class CmdbRepository
    {
        private static Lazy<CmdbRepository> _instance;
        private readonly IRepositoryAdapter<CmdbGraph<CmdbItem, CmdbDependency>> _graphsAdapter;
        private readonly IRepositoryAdapter<CmdbItem> _nodesAdapter;
        private readonly IRepositoryAdapter<CmdbDependency> _edgesAdapter;
        private long _edgeNextSequenceId;
        private long _nodeNextSequenceId;

        public static CmdbRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    bool useMongo;
                    if (Boolean.TryParse(ConfigurationManager.AppSettings["useMongo"], out useMongo) && useMongo)
                    {
                        _instance = new Lazy<CmdbRepository>(()=>new CmdbRepository(GlobalHost.ConnectionManager.GetHubContext<CartoHub>().Clients, new RepositoryAdapterMongoDb<CmdbGraph<CmdbItem, CmdbDependency>>("graphs"), new RepositoryAdapterMongoDb<CmdbItem>("nodes"), new RepositoryAdapterMongoDb<CmdbDependency>("edges")));
                    }
                    else
                    {
                        _instance = new Lazy<CmdbRepository>(() => new CmdbRepository(GlobalHost.ConnectionManager.GetHubContext<CartoHub>().Clients, new RepositoryAdapterStub<CmdbGraph<CmdbItem, CmdbDependency>>(), new RepositoryAdapterStub<CmdbItem>(), new RepositoryAdapterStub<CmdbDependency>()));
                    }
                }
                return _instance.Value;
            }
        }

        private IHubConnectionContext Clients { get; set; }
        public CmdbGraph<CmdbItem, CmdbDependency> Graph { get; private set; }
        public IDictionary<long, ICollection<CmdbAttributeDefinition>> AttributeDefinitions { get; set; }
        public IDictionary<long, CmdbItemCategory> Categories { get; set; }

        private CmdbRepository(IHubConnectionContext clients, IRepositoryAdapter<CmdbGraph<CmdbItem, CmdbDependency>> graphsAdapter, IRepositoryAdapter<CmdbItem> nodesAdapter, IRepositoryAdapter<CmdbDependency> edgesAdapter)
        {
            Clients = clients;
            _graphsAdapter = graphsAdapter;
            _nodesAdapter = nodesAdapter;
            _edgesAdapter = edgesAdapter;

            var applicationCategory = new CmdbItemCategory(1, "Application");
            Categories = new Dictionary<long, CmdbItemCategory> {{applicationCategory.Id, applicationCategory}};

            var applicationTypeAttribute = new CmdbAttributeDefinition {Id = 1, Name = "Application Type", Type = typeof (string), AuthorisedValues = new List<object> {"Desktop", "Service"}};
            var languageAttribute = new CmdbAttributeDefinition { Id = 2, Name = "Language", Type = typeof(string) };
            var operationSystemAttribute = new CmdbAttributeDefinition { Id = 3, Name = "Operation System", Type = typeof(string) };
            var itOwnerAttribute = new CmdbAttributeDefinition { Id = 4, Name = "IT Owner", Type = typeof(string) };
            var urlAttribute = new CmdbAttributeDefinition {Id = 5, Name = "Url Link", Type = typeof (string)};
            var businessOwnerAttribute = new CmdbAttributeDefinition {Id = 6, Name = "Business Owner", Type = typeof (string)};
            var componentVersionAttribute = new CmdbAttributeDefinition {Id = 7, Name = "Version", Type = typeof (string)};
            //var criticalityAttribute = new CmdbAttributeDefinition {Id = 8, Name = "Criticality", Type = typeof (string)}; //non critical, critical, mission critical
            //var vendorAttribute = new CmdbAttributeDefinition {Id = 9, Name = "Vendor", Type = typeof (string)};
            //var wikiAttribute = new CmdbAttributeDefinition {Id = 10, Name = "Wiki Url", Type = typeof (string)};
            //var licencesAttribute = new CmdbAttributeDefinition {Id = 8, Name = "Licences", Type = typeof (List<string>)};

            AttributeDefinitions = new Dictionary<long, ICollection<CmdbAttributeDefinition>>();
            AttributeDefinitions[applicationCategory.Id] = new List<CmdbAttributeDefinition>
                {
                    applicationTypeAttribute,
                    languageAttribute,
                    operationSystemAttribute,
                    itOwnerAttribute,
                    urlAttribute,
                    businessOwnerAttribute,
                    componentVersionAttribute,
                };
        }

        //private void InitDb(CmdbItemCategory applicationCategory, CmdbAttributeDefinition applicationTypeAttribute, CmdbAttributeDefinition languageAttribute, CmdbAttributeDefinition operationSystemAttribute, CmdbAttributeDefinition itOwnerAttribute)
        //{
        //    var RaDaR = new CmdbItem(applicationCategory, 1, "RaDaR") {Description = "Risk Front End"};
        //    RaDaR.Attributes[applicationTypeAttribute.Id] = "Desktop";
        //    RaDaR.Attributes[languageAttribute.Id] = "C#/WPF";
        //    RaDaR.Attributes[operationSystemAttribute.Id] = "Windows";
        //    RaDaR.Attributes[itOwnerAttribute.Id] = "Dave Collis";
        //    var RTS = new CmdbItem(applicationCategory, 2, "RTS") {Description = "Risk Terms Service"};
        //    RTS.Attributes[applicationTypeAttribute.Id] = "Service";
        //    RTS.Attributes[languageAttribute.Id] = "Java";
        //    RTS.Attributes[operationSystemAttribute.Id] = "Linux";
        //    RTS.Attributes[itOwnerAttribute.Id] = "Sam Ratcliff";
        //    var SDS = new CmdbItem(applicationCategory, 3, "SDS") {Description = "Static Data Service"};
        //    SDS.Attributes[applicationTypeAttribute.Id] = "Service";
        //    SDS.Attributes[languageAttribute.Id] = "Java";
        //    SDS.Attributes[operationSystemAttribute.Id] = "Linux";
        //    SDS.Attributes[itOwnerAttribute.Id] = "Ting Hau";

        //    var RaDaR_RTS = new CmdbDependency(1, RaDaR, RTS);
        //    var RTS_SDS = new CmdbDependency(2, RTS, SDS);

        //    _nodesAdapter.Create(RaDaR);
        //    _nodesAdapter.Create(RTS);
        //    _nodesAdapter.Create(SDS);
        //    _edgesAdapter.Create(RaDaR_RTS);
        //    _edgesAdapter.Create(RTS_SDS);
        //}

        public CmdbItem Update(CmdbItem item)
        {
            var currentVertex = Graph.Vertices.FirstOrDefault(v => v.Id == item.Id && v.Version == item.Version);
            if (currentVertex == null)
            {
                throw new Exception();
            }
            item.Version = item.Version + 1;

            _nodesAdapter.Update(item);

            //swap vertices
            Graph.AddVertex(item);
            var outEdges = Graph.OutEdges(currentVertex);
            var inEdges = Graph.InEdges(currentVertex);
            foreach (var outEdge in outEdges)
            {
                outEdge.Source = item;
                Graph.AddEdge(outEdge);
            }
            foreach (var inEdge in inEdges)
            {
                inEdge.Target = item;
                Graph.AddEdge(inEdge);
            }
            Graph.RemoveVertex(currentVertex);
            Clients.All.updateNode(item);

            return item;
        }

        public CmdbItem Create(CmdbItem item)
        {
            var nextId = Interlocked.Increment(ref _nodeNextSequenceId);
            var category = (item != null &&  item.Category != null && Categories.ContainsKey(item.Category.Id)) ? item.Category : Categories.First().Value;
            var name = (item != null && item.Name != null) ? item.Name : string.Empty;
            var newItem = new CmdbItem(category, nextId, name) {GraphId = item.GraphId};

            _nodesAdapter.Create(newItem);

            Graph.AddVertex(newItem);
            Clients.All.createNode(newItem);

            return newItem;
        }

        public bool Delete(long id)
        {
            //TODO archive in a nodes_archive collection?            
            var currentVertex = Graph.Vertices.FirstOrDefault(v => v.Id == id);
            var inEdges = Graph.InEdges(currentVertex);
            var outEdges = Graph.OutEdges(currentVertex);

            foreach (var edge in inEdges)
            {
                _edgesAdapter.Delete(edge.Id);
            }
            foreach (var edge in outEdges)
            {
                _edgesAdapter.Delete(edge.Id);
            }
            _nodesAdapter.Delete(id);
            var ret = Graph.RemoveVertex(currentVertex);
            if (ret)
            {
                Clients.All.deleteNode(currentVertex);
            }
            return ret;
        }

        public CmdbDependency AddEdge(CmdbDependency edge)
        {
            edge.Id = Interlocked.Increment(ref _edgeNextSequenceId);
            edge.Source = Graph.Vertices.First(v => v.Id == edge.SourceId);
            edge.Target = Graph.Vertices.First(v => v.Id == edge.TargetId);

            _edgesAdapter.Create(edge);

            Graph.AddEdge(edge);
            Clients.All.createLink(edge);
            return edge;
        }

        public bool DeleteLink(long id)
        {
            _edgesAdapter.Delete(id);

            var edge = Graph.Edges.First(e => e.Id == id);
            var ret = Graph.RemoveEdge(edge);
            if (ret)
            {
                Clients.All.deleteLink(edge);
            }
            return ret;
        }

        public IEnumerable<CmdbGraph<CmdbItem, CmdbDependency>> ReadAll()
        {
            return _graphsAdapter.ReadAll();
        }

        public CmdbGraph<CmdbItem, CmdbDependency> ReadGraph(long graphId)
        {
            if (Graph == null || Graph.Id != graphId)
            {
                _nodeNextSequenceId = _nodesAdapter.ReadMaxId() + 1;
                _edgeNextSequenceId = _edgesAdapter.ReadMaxId() + 1;

                var g = _graphsAdapter.Read(graphId);

                var nodes = _nodesAdapter.ReadAll(graphId).ToList();
                var edges = _edgesAdapter.ReadAll(graphId).ToList();

                g.AddVertexRange(nodes);

                var nodesMap = nodes.ToDictionary(n => n.Id);
                foreach (var edge in edges)
                {
                    edge.Source = nodesMap[edge.SourceId];
                    edge.Target = nodesMap[edge.TargetId];
                }
                g.AddEdgeRange(edges);

                Graph = g;
            }
            return Graph;
        }
    }
}