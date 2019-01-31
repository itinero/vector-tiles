// The MIT License (MIT)

// Copyright (c) 2017 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

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

            var segmentLayerConfig = config.SegmentLayerConfig;
            if (segmentLayerConfig != null && segmentLayerConfig.Name == null) { throw new ArgumentException($"{nameof(config.SegmentLayerConfig)} configuration has no name set."); }

            var tile = new Tile(tileId);
            var diffX = (tile.Top - tile.Bottom);
            var diffY = (tile.Right - tile.Left);
            var marginX = diffX / 1024;
            var marginY = diffY / 1024;

            var tileBox = new LocalGeo.Box(tile.Bottom - marginY, tile.Left - marginX, 
                tile.Top + marginY, tile.Right + marginX);
            var segmentLayer = segmentLayerConfig?.NewLayer(routerDb);
            
            // initialize vertex layers.
            var vertexLayers = new List<VertexLayer>();
            if (config.VertexLayerConfigs != null)
            {
                foreach (var vertexLayerConfig in config.VertexLayerConfigs)
                {
                    vertexLayers.Add(vertexLayerConfig.NewLayer());
                }
            }

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
                    if (vertexLayer.Config.IncludeFunc(vertex))
                    {
                        vertexLayer.Vertices.Add(vertex);
                    }
                }
                
                if (segmentLayer == null) continue;
                
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

                    // check if this edge needs to be included or not.
                    if (segmentLayerConfig?.IncludeProfileFunc != null && !segmentLayerConfig.IncludeProfileFunc(edgeData.Profile, edgeData.MetaId))
                    { // include profile returns false
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

                            segmentLayer.Segments.Add(new Segment()
                            {
                                Meta = edgeData.MetaId,
                                Profile = edgeData.Profile,
                                Shape = shape.ToArray(),
                                EdgeId = edgeEnumerator.Id,
                            });
                            shape.Clear();
                            previous = false;
                        }
                    }

                    if (shape.Count >= 2)
                    {
                        segmentLayer.Segments.Add(new Segment()
                        {
                            Meta = edgeData.MetaId,
                            Profile = edgeData.Profile,
                            EdgeId = edgeEnumerator.Id,
                            Shape = shape.ToArray()
                        });
                        shape.Clear();
                    }
                }
            }

            var layers = new List<Layer>(vertexLayers);
            if (segmentLayer !=null) layers.Add(segmentLayer);
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
    }
}