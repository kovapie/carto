using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using QuickGraph;
using carto.Api;

namespace carto.Models
{
    public class CmdbRepository
    {
        private static CmdbRepository _instance;
        private readonly MongoDatabase _database;
        private readonly MongoCollection<CmdbItem> _dbNodes;
        private readonly MongoCollection<CmdbDependency> _dbEdges;

        public static CmdbRepository Instance
        {
            get { return _instance ?? (_instance = new CmdbRepository()); }
        }

        public BidirectionalGraph<CmdbItem, CmdbDependency> Graph { get; private set; }
        public IDictionary<long, ICollection<CmdbAttributeDefinition>> AttributeDefinitions { get; set; }
        public IDictionary<long, CmdbItemCategory> Categories { get; set; }

        private CmdbRepository()
        {
            var client = new MongoClient("mongodb://carto:carto@riskdevlx1.london.daiwa.global:27017/carto");
            _database = client.GetServer().GetDatabase("carto");
            _dbNodes = _database.GetCollection<CmdbItem>("nodes");
            _dbEdges = _database.GetCollection<CmdbDependency>("edges");
            
            var applicationCategory = new CmdbItemCategory(1, "Application");
            Categories = new Dictionary<long, CmdbItemCategory> {{applicationCategory.Id, applicationCategory}};

            var applicationTypeAttribute = new CmdbAttributeDefinition {Id = 1, Name = "Application Type", Type = typeof (string), AuthorisedValues = new List<object> {"Desktop", "Service"}};
            var languageAttribute = new CmdbAttributeDefinition { Id = 2, Name = "Language", Type = typeof(string) };
            var operationSystemAttribute = new CmdbAttributeDefinition { Id = 3, Name = "Operation System", Type = typeof(string) };
            var itOwnerAttribute = new CmdbAttributeDefinition { Id = 4, Name = "IT Owner", Type = typeof(string) };
            var urlAttribute = new CmdbAttributeDefinition {Id = 5, Name = "Url Link", Type = typeof (string)};
            var businessOwnerAttribute = new CmdbAttributeDefinition {Id = 6, Name = "Business Owner", Type = typeof (string)};
            var componentVersionAttribute = new CmdbAttributeDefinition {Id = 7, Name = "Version", Type = typeof (string)};
            var criticalityAttribute = new CmdbAttributeDefinition {Id = 8, Name = "Criticality", Type = typeof (string)}; //non critical, critical, mission critical
            var vendorAttribute = new CmdbAttributeDefinition {Id = 9, Name = "Vendor", Type = typeof (string)};
            var wikiAttribute = new CmdbAttributeDefinition {Id = 10, Name = "Wiki Url", Type = typeof (string)};
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

            //InitDb(applicationCategory, applicationTypeAttribute, languageAttribute, operationSystemAttribute, itOwnerAttribute);
            
            var g = new BidirectionalGraph<CmdbItem, CmdbDependency>();

            var nodes = _dbNodes.FindAll().ToList();
            var edges = _dbEdges.FindAll().ToList();

            g.AddVertexRange(nodes);

            var nodesMap = nodes.ToDictionary(n => n.Id);
            foreach (var edge in edges)
            {
                edge.Source = nodesMap[edge.SourceId];
                edge.Target= nodesMap[edge.TargetId];
            }
            g.AddEdgeRange(edges);
            
            Graph = g;
        }

        private void InitDb(CmdbItemCategory applicationCategory, CmdbAttributeDefinition applicationTypeAttribute, CmdbAttributeDefinition languageAttribute, CmdbAttributeDefinition operationSystemAttribute, CmdbAttributeDefinition itOwnerAttribute)
        {
            var RaDaR = new CmdbItem(applicationCategory, 1, "RaDaR") {Description = "Risk Front End"};
            RaDaR.Attributes[applicationTypeAttribute.Id] = "Desktop";
            RaDaR.Attributes[languageAttribute.Id] = "C#/WPF";
            RaDaR.Attributes[operationSystemAttribute.Id] = "Windows";
            RaDaR.Attributes[itOwnerAttribute.Id] = "Dave Collis";
            var RTS = new CmdbItem(applicationCategory, 2, "RTS") {Description = "Risk Terms Service"};
            RTS.Attributes[applicationTypeAttribute.Id] = "Service";
            RTS.Attributes[languageAttribute.Id] = "Java";
            RTS.Attributes[operationSystemAttribute.Id] = "Linux";
            RTS.Attributes[itOwnerAttribute.Id] = "Sam Ratcliff";
            var SDS = new CmdbItem(applicationCategory, 3, "SDS") {Description = "Static Data Service"};
            SDS.Attributes[applicationTypeAttribute.Id] = "Service";
            SDS.Attributes[languageAttribute.Id] = "Java";
            SDS.Attributes[operationSystemAttribute.Id] = "Linux";
            SDS.Attributes[itOwnerAttribute.Id] = "Ting Hau";

            var RaDaR_RTS = new CmdbDependency(1, RaDaR, RTS);
            var RTS_SDS = new CmdbDependency(2, RTS, SDS);

            _dbNodes.Insert(RaDaR);
            _dbNodes.Insert(RTS);
            _dbNodes.Insert(SDS);
            _dbEdges.Insert(RaDaR_RTS);
            _dbEdges.Insert(RTS_SDS);
        }

        public CmdbItem Update(CmdbItem item)
        {
            //for lock free thread safe version, clone graph, update, and swap, checking the version number
            var currentVertex = Graph.Vertices.FirstOrDefault(v => v.Id == item.Id && v.Version == item.Version);
            if (currentVertex == null)
            {
                throw new Exception();
            }
            item.Version = item.Version + 1;
            
            _dbNodes.Save(item);

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
            return item;
        }

        public CmdbItem Create(CmdbItem item)
        {
            //TODO make that thread safe (either rely on mongo ObjectID or have a next Id on the graph with Interlocked.Increment() 
            var nextId = Graph.Vertices.Select(v => v.Id).Max() + 1;
            var category = (item != null &&  item.Category != null && Categories.ContainsKey(item.Category.Id)) ? item.Category : Categories.First().Value;
            var name = (item != null && item.Name != null) ? item.Name : string.Empty;
            var newItem = new CmdbItem(category, nextId, name);
            
            _dbNodes.Insert(newItem);

            Graph.AddVertex(newItem);


            return newItem;
        }

        public bool Delete(long id)
        {
            //TODO archive in a nodes_archive collection?
            var query = Query<CmdbItem>.EQ(c => c.Id, id);
            _dbNodes.Remove(query);

            var currentVertex = Graph.Vertices.FirstOrDefault(v => v.Id == id);
            return Graph.RemoveVertex(currentVertex);
        }

        public CmdbDependency AddEdge(CmdbDependency edge)
        {
            var nextId = Graph.Edges.Select(e => e.Id).Max() + 1;
            edge.Id = nextId;
            edge.Source = Graph.Vertices.First(v => v.Id == edge.SourceId);
            edge.Target = Graph.Vertices.First(v => v.Id == edge.TargetId);

            _dbEdges.Insert(edge);

            Graph.AddEdge(edge);
            return edge;
        }

        public bool DeleteLink(long id)
        {
            var query = Query<CmdbDependency>.EQ(c => c.Id, id);
            _dbEdges.Remove(query);

            var edge = Graph.Edges.First(e => e.Id == id);
            return Graph.RemoveEdge(edge);
        }
    }

}