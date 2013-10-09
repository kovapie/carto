using System;
using System.Collections.Generic;
using System.Linq;
using QuickGraph;
using carto.Api;

namespace carto.Models
{
    public class CmdbRepository
    {
        private static CmdbRepository _instance;

        public static CmdbRepository Instance
        {
            get { return _instance ?? (_instance = new CmdbRepository()); }
        }

        public BidirectionalGraph<CmdbItem, CmdbDependency> Graph { get; private set; }
        public IDictionary<long, ICollection<CmdbAttributeDefinition>> AttributeDefinitions { get; set; }
        public IDictionary<long, CmdbItemCategory> Categories { get; set; }

        private CmdbRepository()
        {
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
            
            var RaDaR_RTS = new CmdbDependency (1, RaDaR, RTS);
            var RTS_SDS = new CmdbDependency(2, RTS, SDS);

            var g = new BidirectionalGraph<CmdbItem, CmdbDependency>();
            g.AddVerticesAndEdge(RaDaR_RTS);
            g.AddVerticesAndEdge(RTS_SDS);
            Graph = g;
        }

        public CmdbItem Update(CmdbItem item)
        {
            //for thread safe version, clone graph, update, and swap, checking the version number
            var currentVertex = Graph.Vertices.FirstOrDefault(v => v.Id == item.Id && v.Version == item.Version);
            if (currentVertex == null)
            {
                throw new Exception();
            }
            item.Version = item.Version + 1;
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
            var nextId = Graph.Vertices.Select(v => v.Id).Max() + 1;
            var category = (item != null &&  item.Category != null && Categories.ContainsKey(item.Category.Id)) ? item.Category : Categories.First().Value;
            var name = (item != null && item.Name != null) ? item.Name : string.Empty;
            var newItem = new CmdbItem(category, nextId, name);
            Graph.AddVertex(newItem);
            return newItem;
        }

        public bool Delete(long id)
        {
            var currentVertex = Graph.Vertices.FirstOrDefault(v => v.Id == id);
            return Graph.RemoveVertex(currentVertex);
        }

        public CmdbDependency AddEdge(CmdbDependency edge)
        {
            var nextId = Graph.Edges.Select(e => e.Id).Max() + 1;
            edge.Id = nextId;
            edge.Source = Graph.Vertices.First(v => v.Id == edge.SourceId);
            edge.Target = Graph.Vertices.First(v => v.Id == edge.TargetId);
            Graph.AddEdge(edge);
            return edge;
        }

        public bool DeleteLink(long id)
        {
            var edge = Graph.Edges.First(e => e.Id == id);
            return Graph.RemoveEdge(edge);
        }
    }

}