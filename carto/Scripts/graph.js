var width = 960,
    height = 500;

var svg = d3.select("svg")
    .attr("width", "100%")
    .attr("height", height)
    .on("mousedown", onMouseDown)
    .on("mouseup", onMouseUp)
    .on("mousemove", onMouseMove);

var force = d3.layout.force()
    .size([width, height])
    .linkDistance(80)
    .charge(-1000)
    .gravity(0.2)
    .on("tick", tick);

var zoom = d3.behavior.zoom().scaleExtent([0.5, 2]).on("zoom", onZoom);
zoom(d3.select(".overlay"));
    
var vis = d3.select(".vis");
var dragline = svg.select(".dragline");

var viewModel = new GraphViewModel();

var link = d3.select("#links_container").selectAll(".link"),
    node = d3.select("#nodes_container").selectAll(".node");

$(function() {
    $(document)
        .keydown(onKeyDown)
        .keyup(onKeyUp);

    ko.applyBindings(viewModel);
    viewModel.init();

    var pubsubhub = $.connection.cartoHub;

    pubsubhub.client.createNode = function(node) {
        viewModel.addNode(new CmdbViewModel(node));
    };
    pubsubhub.client.updateNode = function (node) {
        viewModel.updateNode(new CmdbViewModel(node));
    };
    pubsubhub.client.deleteNode = function (node) {
        viewModel.deleteNode(new CmdbViewModel(node));
    };
    pubsubhub.client.createLink = function (link) {
        link.source = ko.utils.arrayFirst(viewModel.nodes(), function (item) { return(item.id === link.SourceId); });
        link.target = ko.utils.arrayFirst(viewModel.nodes(), function (item) { return(item.id === link.TargetId); });
        viewModel.addLink(new LinkViewModel(link));
    };
    pubsubhub.client.deleteLink = function (link) {
        viewModel.deleteLink(new LinkViewModel(link));
    };

    $.connection.hub.start();
    
    d3.select("#charge").on("change",function() {
        d3.select("#charge_label").text("Charge: " + d3.format("f")(this.value));
        force.charge(-this.value).start();
    });
    d3.select("#gravity").on("change", function () {
        d3.select("#gravity_label").text("Gravity: " + d3.format("g")(this.value));
        force.gravity(this.value).start();
    });
    d3.select("#distance").on("change", function () {
        d3.select("#distance_label").text("Distance: " + d3.format("f")(this.value));
        force.linkDistance(this.value).start();
    });
    d3.select("#strength").on("change", function () {
        d3.select("#strength_label").text("Strength: " + d3.format("g")(this.value));
        force.linkStrength(this.value).start();
    });
    d3.select("#friction").on("change", function () {
        d3.select("#friction_label").text("Friction: " + d3.format("g")(this.value));
        force.friction(this.value).start();
    });

    $("#options_trigger").hover(function () {
        $("#options").fadeIn();
    }, function () {
        $("#options").fadeOut();
    });
});

function onZoom() {
    vis.attr("transform", "translate(" + d3.event.translate + ")scale(" + d3.event.scale + ")");
}

function tick() {
    link.attr("d", function (d) {
        var deltaX = d.target.x - d.source.x,
            deltaY = d.target.y - d.source.y,
            dist = Math.sqrt(deltaX * deltaX + deltaY * deltaY),
            normX = deltaX / dist,
            normY = deltaY / dist,
            //sourcePadding = d.left ? 17 : 12,
            //targetPadding = d.right ? 17 : 12,
            sourcePadding = 12,
            targetPadding = 17,
            sourceX = d.source.x + (sourcePadding * normX),
            sourceY = d.source.y + (sourcePadding * normY),
            targetX = d.target.x - (targetPadding * normX),
            targetY = d.target.y - (targetPadding * normY);
        return 'M' + sourceX + ',' + sourceY + 'L' + targetX + ',' + targetY;
    });

    node.attr('transform', function (d) {
        return 'translate(' + d.x + ',' + d.y + ')';
    });
}

function drawgraph(graphdata) {
    viewModel.update(graphdata);
    redraw();
}
    
function redraw() {
    force.nodes(viewModel.nodes())
        .links(viewModel.links())
        .start();

    link = link.data(viewModel.links());
    link.exit().remove();
    link.enter().append("path").attr("class", "link");
    link.classed("selected", function (d) {
        return d === viewModel.selectedLink();
    });
    link.on("click", onLinkClicked);

    node = node.data(viewModel.nodes(), function (d) { return d.id; });
    node.exit().remove();

    var g = node.enter().append("g").attr("class", "node");
    g.append("circle").attr("r", 12);
    g.append("text").attr("class", "label");
    g.append("title");
    node.classed("selected", function (d) {
        return d === viewModel.selectedItem();
    });
    node.classed("Desktop", function (d) { return d.applicationType() === "Desktop"; });
    node.classed("Service", function (d) { return d.applicationType() === "Service"; });
    node.select("text").text(function (d) { return d.name(); });
    node.select("title").text(function (d) { return d.description(); });

    node.on("click", click)
        .on("mousedown", onNodeMouseDown)
        .on("mouseup", onNodeMouseUp)
        .call(force.drag);

    force.drag().on("dragstart", dragstart);

    function dragstart(d) {
        d.fixed = true;
    }

    function click(n) {
        //if (d3.event.defaultPrevented) return;
        if (d3.event.shiftKey && n.fixed) {
            n.fixed = false;
        }
        if (n === viewModel.selectedItem()) {
            viewModel.selectedItem(null);
        } else {
            viewModel.selectNode(n);
        }
        redraw();
    }
        
    function onLinkClicked(l) {
        if (l === viewModel.selectedLink()) {
            viewModel.selectedLink(null);
        } else {
            viewModel.selectLink(l);
        }
        redraw();
    }

    function onNodeMouseDown(node) {
        if (d3.event.ctrlKey) {
            viewModel.fromItem(node);
            var scale = zoom.scale();
            var translate = zoom.translate();
            var fromX = viewModel.fromItem().x * scale + translate[0];
            var fromY = viewModel.fromItem().y * scale + translate[1];
            dragline
                .classed('hidden', false)
                .attr('d', 'M' + fromX + ',' + fromY + 'L' + fromX + ',' + fromY);
            redraw();
        }
    }

    function onNodeMouseUp(node) {
        if (viewModel.fromItem() && d3.event.ctrlKey) {
            if (viewModel.fromItem() !== node) {
                createLink({ source: viewModel.fromItem(), target: node, GraphId:viewModel.selectedGraph().id });
            }
        }
        viewModel.resetDragLine();
    }
}

function onSave(node) {
    if (!node) {
        node = viewModel.selectedItem();
    }
    var  vertex = node.toDto();
    $.ajax({
        type: "PUT",
        contentType: "application/json;charset=utf-8",
        url: "api/graph/node/" + vertex.Id,
        data: JSON.stringify(vertex),
    }).done(function (savedNode) {
        viewModel.updateNode(new CmdbViewModel(savedNode));
    });
}
    
function onCreate(point) {
    var vertex={GraphId:viewModel.selectedGraph().id};
    if (viewModel.selectedItem() != null) {
        vertex = viewModel.selectedItem().toDto();
    }
    $.ajax({
        type: "POST",
        contentType: "application/json;charset=utf-8",
        url: "api/graph/node",
        data: JSON.stringify(vertex),
    }).done(function (node) {
        var nodevm = new CmdbViewModel(node);
        var scale = zoom.scale();
        var translate = zoom.translate();
        nodevm.x = (point[0] - translate[0]) /scale;
        nodevm.y = (point[1] - translate[1]) /scale;
        viewModel.addNode(nodevm,true);
    });
}
    
function onDelete() {
    var vertex = viewModel.selectedItem();
    $.ajax({
        type: "DELETE",
        url: "api/graph/node/" + vertex.id,
    }).done(function (isDeleted) {
        if (isDeleted) {
            viewModel.deleteNode(vertex);
        }
    });
}

function createLink(link) {
    $.ajax({
        type: "POST",
        contentType: "application/json;charset=utf-8",
        url: "api/graph/link",
        data: JSON.stringify(link),
    }).done(function (edge) {
        edge.source = link.source;
        edge.target = link.target;
        var linkvm = new LinkViewModel(edge);            
        viewModel.addLink(linkvm,true);
    });
}

function onDeleteLink() {
    var selectedLink = viewModel.selectedLink();
    $.ajax({
        type: "DELETE",
        url: "api/graph/link/" + selectedLink.id,
    }).done(function (isDeleted) {
        if (isDeleted) {
            viewModel.deleteLink(selectedLink);
        }
    });
}

function onKeyDown(event) {
    switch (event.which) {
        case 17: //ctrl
            node.on("mousedown.drag", null);
            svg.classed("ctrl", true);
            break;
        case 46: //del
            if (event.ctrlKey) {
                if (viewModel.selectedItem()) {
                    onDelete();
                }
                if (viewModel.selectedLink()) {
                    onDeleteLink();
                }
            }
            break;
        case 45: //ins
            if (event.ctrlKey) {
                onCreate({},true);
            }
            break;
        case 109: //num -
        case 107: //num +
            //TODO expand/collapse node
            break;
    }
}

function onKeyUp(event) {
    if (event.which === 17) {
        node.call(force.drag);
        svg.classed("ctrl", false);
        viewModel.resetDragLine();
    }
}

function onMouseDown() {
    if (d3.event.ctrlKey && !viewModel.fromItem()) {
        onCreate(d3.mouse(this),true);
    }
}

function onMouseUp() {
    viewModel.resetDragLine();
}
    
function onMouseMove() {
    if (!viewModel.fromItem()) return;
    var scale = zoom.scale();
    var translate = zoom.translate();
    var fromX = viewModel.fromItem().x * scale + translate[0];
    var fromY = viewModel.fromItem().y * scale + translate[1];
    dragline.attr('d', 'M' + fromX + ',' + fromY + 'L' + d3.mouse(this)[0] + ',' + d3.mouse(this)[1]);
    redraw();
}

function CmdbViewModel(node) {
    var self = this;
    self.innerNode = node;
    self.id = node.Id;
    self.version = node.Version;
    self.name = ko.observable(node.Name);
    self.description = ko.observable(node.Description);
    self.applicationType = ko.observable(node.Attributes[1]);
    self.criticality = ko.observable(node.Attributes[8]);
    self.language = ko.observableArray(node.Attributes[2]);
    self.operatingSystem = ko.observable(node.Attributes[3]);
    self.itOwner = ko.observable( node.Attributes[4]);
    self.businessOwner = ko.observable(node.Attributes[6]);
    self.url = ko.observable(node.Attributes[5]);
    self.isVendor = ko.observable(node.Attributes[9]);

    self.isDirty = ko.computed(function () { return { name: self.name(), description: self.description(), applicationType: self.applicationType(), criticality: self.criticality(), language: self.language(), operatingSystem: self.operatingSystem(), itOwner: self.itOwner(), businessOwner: self.businessOwner(), url: self.url(), isVendor:self.isVendor() }; });

    self.toDto = function () {
        self.innerNode.Name = self.name();
        self.innerNode.Description = self.description();
        self.innerNode.Attributes[1] = self.applicationType();
        //self.innerNode.Attributes[2] = self.language(); //got deserialized into Newtonsolft Jcontainer and mess with mongodb - TODO: fix the Web API deserialization on the server side
        self.innerNode.Attributes[3] = self.operatingSystem();
        self.innerNode.Attributes[4] = self.itOwner();
        self.innerNode.Attributes[5] = self.url();
        self.innerNode.Attributes[6] = self.businessOwner();
        self.innerNode.Attributes[8] = self.criticality();
        self.innerNode.Attributes[9] = self.isVendor();
        return self.innerNode;
    };
}
    
function LinkViewModel(link) {
    var self = this;
    this.innerLink = link;
    this.id = link.Id;
    this.version = link.version;
    this.source = link.source;
    this.target = link.target;

    this.toDto = function() {
        self.innerLink.SourceId = self.source().id;
        self.innerLink.TargetId = self.target().id;
        return self.innerLink;
    };
}

function GraphViewModel() {
    var self = this;
    var subs = [];
        
    self.graphs = ko.observableArray();
    self.nodes = ko.observableArray();
    self.links = ko.observableArray();

    self.selectedGraph = ko.observable();
    self.filter = ko.observable();
    self.selectedItem = ko.observable();
    self.selectedLink = ko.observable();
    self.canSave = ko.computed(function () { return self.selectedItem() != null; });

    self.fromItem = ko.observable();

    self.init = function () {
        $.getJSON("api/graph/graph")
            .done(function (graphs) { self.graphs(graphs); })
            .fail(function(error) {alert(error);});
    };

    self.selectGraph = function (graph) {
        self.selectedGraph(graph);
        self.loadGraph();
    };

    self.loadGraph = function () {
        var filterUrl = self.filter() ? "&$filter=" + self.filter() : "";
        var url = "api/graph/node?graphId=" + self.selectedGraph().id + filterUrl;
        $.getJSON(url)
            .done(drawgraph)
            .fail(function (error) { alert(error); });
    };

    self.update = function (graphdata) {
        ko.utils.arrayForEach(subs, function (sub) { sub.dispose(); });
        self.nodes.removeAll();
        self.links.removeAll();
        self.selectedItem(null);
        self.selectedLink(null);
        var alllinks = new Array();
        var nodeMap = {};

        for (var i = 0; i < graphdata.length; i++) {
            var item = graphdata[i];
            var vertex = item.vertex;
            var edges = item.edges;
            var itemviewmodel = new CmdbViewModel(vertex);
            nodeMap[vertex.Id] = itemviewmodel;
            self.nodes.push(itemviewmodel);
            subs.push(itemviewmodel.isDirty.subscribe(function () {
                onSave(this);
            }, itemviewmodel, "change"));
            for (var j = 0; j < edges.length; j++) {
                alllinks.push(edges[j]);
            }
        }

        for (var k = 0; k < alllinks.length; k++) {
            var edge = alllinks[k];
            if (edge.TargetId in nodeMap) {
                edge.target = nodeMap[edge.TargetId];
                edge.source = nodeMap[edge.SourceId];
                self.links.push(new LinkViewModel(edge));
            }
        }
    };

    self.selectNode = function(node) {
        self.selectedLink(null);
        self.selectedItem(node);
    };

    self.selectLink = function(link) {
        self.selectedItem(null);
        self.selectedLink(link);
    };

    this.updateNode = function (node) {
        var currentNode = ko.utils.arrayFirst(self.nodes(), function (item) {
            return (item.id === node.id);
        });
        if (!currentNode) {
            self.addNode(node);
        } else if (currentNode.version < node.version) {
            //TODO remove and dispose subscription to old node
            subs.push(node.isDirty.subscribe(function () {
                onSave(this);
            }, node, "change"));
            node.x = currentNode.x;
            node.y = currentNode.y;
            self.nodes.replace(currentNode, node);
            for (var j = 0; j < this.links().length; j++) {
                var currentLink = this.links()[j];
                if (currentLink.target === currentNode) {
                    currentLink.target = node;
                }
                if (currentLink.source === currentNode) {
                    currentLink.source = node;
                }
            }
            if (self.selectedItem() === currentNode) {
                self.selectNode(node);
            }
            redraw();
        }
    };

    this.addNode = function (node,isSelected) {
        var currentNode = ko.utils.arrayFirst(self.nodes(), function (item) {
            return (item.id === node.id);
        });
        if (!currentNode) {
            this.nodes.push(node);
            subs.push(node.isDirty.subscribe(function() {
                onSave(this);
            }, node, "change"));
            if (isSelected) {
                self.selectNode(node);
            }
            redraw();
        }
    };

    this.deleteNode = function (node) {
        //TODO remove and dispose subscription for this node (based on the target?)
        var currentNode = ko.utils.arrayFirst(self.nodes(), function(item) {
            return (item.id === node.id);
        });
        if (currentNode) {
            self.links.remove(function(item) {
                return (item.source === currentNode || item.target === currentNode);
            });
            self.nodes.remove(currentNode);
            if (viewModel.selectedItem() === node) {
                viewModel.selectedItem(null);
            }
            redraw();
        }
    };

    this.addLink = function (link,isSelected) {
        var currentLink = ko.utils.arrayFirst(self.links(), function (item) {
            return (item.id === link.id);
        });
        if (!currentLink) {
            self.links.push(link);
            if (isSelected) {
                self.selectLink(link);
            }
            redraw();
        }
    };

    this.deleteLink = function (link) {
        var currentLink = ko.utils.arrayFirst(self.links(), function (item) {
            return (item.id === link.id);
        });
        if (currentLink) {
            self.links.remove(currentLink);
            if (viewModel.selectedLink() === currentLink) {
                viewModel.selectedLink(null);
            }
            redraw();
        }
    };

    this.resetDragLine = function() {
        this.fromItem(null);
        dragline.classed("hidden", true);
    };
}
