using Itinero.Algorithms.Search.Hilbert;
using Itinero.Data.Network;
using Itinero.Graphs.Geometric;
using Itinero.LocalGeo;
using Itinero.VectorTiles.Layers;
using Itinero.VectorTiles.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using Itinero.Attributes;
using Attribute = Itinero.Attributes.Attribute;

namespace Itinero.VectorTiles
{
    /// <summary>
    /// Contains routerdb extensions.
    /// </summary>
    public static class RouterDbExtensions
    {
        /// <summary>
        /// Extracts a segment layer for the given tile.
        /// </summary>
        public static IEnumerable<Layer> ExtractLayers(this RouterDb routerDb, ulong tileId,
            VectorTileConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            var tile = new Tile(tileId);
            var diffX = (tile.Top - tile.Bottom);
            var diffY = (tile.Right - tile.Left);
            var marginX = diffX / 1024;
            var marginY = diffY / 1024;

            var tileBox = new LocalGeo.Box(tile.Bottom - marginY, tile.Left - marginX, 
                tile.Top + marginY, tile.Right + marginX);
            
            // initialize vertex layers.
            var vertexLayers = new List<VertexLayer>();
            if (config.VertexLayerConfigs != null)
            {
                foreach (var vertexLayerConfig in config.VertexLayerConfigs)
                {
                    vertexLayers.Add(vertexLayerConfig.NewLayer());
                }
            }
            
            // initialize edge layers.
            var edgeLayers = new List<EdgeLayer>();
            if (config.EdgeLayerConfigs != null)
            {
                foreach (var edgeLayerConfig in config.EdgeLayerConfigs)
                {
                    edgeLayers.Add(edgeLayerConfig.NewLayer());
                }
            }

            if (edgeLayers.Count == 0 && vertexLayers.Count == 0) return Enumerable.Empty<Layer>();

            var vertices = HilbertExtensions.Search(routerDb.Network.GeometricGraph,
                tileBox.MinLat - diffY, tileBox.MinLon - diffX, 
                tileBox.MaxLat + diffY, tileBox.MaxLon + diffX);
            var edges = new HashSet<long>();

            var edgeEnumerator = routerDb.Network.GetEdgeEnumerator();
            foreach (var vertex in vertices)
            {
                var coordinateFrom = routerDb.Network.GetVertex(vertex);

                if (!edgeEnumerator.MoveTo(vertex)) continue;

                // add vertex to each layer that wants it.
                foreach (var vertexLayer in vertexLayers)
                {
                    if (vertexLayer.Config.GetAttributesFunc(vertex, tile.Zoom) != null)
                    {
                        vertexLayer.Vertices.Add(new Vertex()
                        {
                            Latitude = coordinateFrom.Latitude,
                            Longitude = coordinateFrom.Longitude,
                            Id = vertex
                        });
                    }
                }
                
                edgeEnumerator.Reset();
                while (edgeEnumerator.MoveNext())
                {
                    if (edges.Contains(edgeEnumerator.Id))
                    {
                        continue;
                    }
                    edges.Add(edgeEnumerator.Id);
                    
                    // loop over shape.
                    var edgeData = edgeEnumerator.Data;

                    // build a list of layers to included or not.
                    var include = edgeLayers.Where(l => l.Config.GetAttributesFunc(edgeEnumerator.Id, tile.Zoom) != null)
                        .ToList();
                    if (include.Count == 0)
                    { // nothing to include.
                        continue;
                    }

                    // get shape.
                    var coordinateTo = routerDb.Network.GetVertex(edgeEnumerator.To);
                    var shape = new List<Coordinate>();
                    var enumShape = routerDb.Network.GetShape(edgeEnumerator.Current);
                    
                    // reverse shape if edge is reversed.
                    if (edgeEnumerator.DataInverted)
                    {
                        enumShape.Reverse();
                    }

                    // split at tile edges.
                    var previous = false;
                    for (var i = 0; i < enumShape.Count; i++)
                    {
                        var location = enumShape[i];
                        if (tileBox.Overlaps(location.Latitude, location.Longitude))
                        {
                            if (previous == false && i > 0)
                            { // come up with intersection point and add that first.
                                var intersection = tileBox.Intersection(new Line(location, enumShape[i - 1]));
                                if (intersection != null)
                                {
                                    shape.Add(intersection.Value);
                                }
                            }

                            // add location.
                            shape.Add(location);
                            previous = true;
                        }
                        else if (previous)
                        { // come up with intersection point and add that as last point.
                            var intersection = tileBox.Intersection(new Line(location, enumShape[i - 1]));
                            if (intersection != null)
                            {
                                shape.Add(intersection.Value);
                            }

                            foreach (var layer in include)
                            {
                                layer.Edges.Add(new Edge()
                                {
                                    Shape = shape.ToArray(),
                                    EdgeId = edgeEnumerator.Id,
                                });
                            }
                            shape.Clear();
                            previous = false;
                        }
                    }

                    if (shape.Count >= 2)
                    {
                        foreach (var layer in include)
                        {
                            layer.Edges.Add(new Edge()
                            {
                                Shape = shape.ToArray(),
                                EdgeId = edgeEnumerator.Id,
                            });
                        }
                        shape.Clear();
                    }
                }
            }

            var layers = new List<Layer>(vertexLayers);
            layers.AddRange(edgeLayers);
            return layers;
        }

        /// <summary>
        /// Extracts a vector tile of the given tile.
        /// </summary>
        public static VectorTile ExtractTile(this RouterDb routerDb, ulong tileId, 
            VectorTileConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            var layers = routerDb.ExtractLayers(tileId, config);

            return new VectorTile()
            {
                Layers = new List<Layer>(layers),
                TileId = tileId
            };
        }

        /// <summary>
        /// Extracts attributes associated with the given vertex.
        /// </summary>
        /// <param name="routerDb">The router db.</param>
        /// <param name="vertex">The vertex.</param>
        /// <returns>An enumerable of attributes.</returns>
        public static IEnumerable<Attribute> GetVertexAttributes(this RouterDb routerDb, uint vertex)
        {
            foreach (var vertexMetaName in routerDb.VertexData.Names)
            {
                var vertexMeta = routerDb.VertexData.Get(vertexMetaName);
                var rawData = vertexMeta.GetRaw(vertex);
                var value = string.Empty;
                if (rawData != null)
                {
                    value = rawData.ToInvariantString();
                }
                yield return new Attribute()
                {
                    Key = vertexMetaName,
                    Value = value
                };
            }

            var attributes = routerDb.VertexMeta?[vertex];
            if (attributes == null) yield break;
            foreach (var a in attributes)
            {
                yield return a;
            }
        }
        
        /// <summary>
        /// Gets all data from the edge meta collections for the given segment.
        /// </summary>
        /// <param name="routerDb">The router db.</param>
        /// <param name="edgeId">The edge.</param>
        /// <returns>An enumerable of attributes associated with the given edge.</returns>
        public static IEnumerable<Attribute> GetEdgeAttributes(this RouterDb routerDb, uint edgeId)
        {
            var edge = routerDb.Network.GetEdge(edgeId);

            var profileMeta = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            if (profileMeta != null)
            {
                foreach (var a in profileMeta)
                {
                    yield return a;
                }
            }

            var edgeMeta = routerDb.EdgeMeta.Get(edge.Data.MetaId);
            if (edgeMeta != null)
            {
                foreach (var a in edgeMeta)
                {
                    yield return a;
                }
            }

            if (routerDb.EdgeData == null) yield break;
            var edgeData = routerDb.EdgeData;
            foreach (var metaName in routerDb.EdgeData.Names)
            {
                var edgeMetaCollection = edgeData.Get(metaName);
                var edgeMetaData = edgeMetaCollection.GetRaw(edgeId);
                var key = metaName;
                var value = string.Empty;
                if (edgeMetaData != null)
                {
                    value = edgeMetaData.ToInvariantString();
                }

                yield return new Attribute(key, value);
            }
        }
    }
}