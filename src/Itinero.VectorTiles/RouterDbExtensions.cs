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

namespace Itinero.VectorTiles
{
    /// <summary>
    /// Contains routerdb extensions.
    /// </summary>
    public static class RouterDbExtensions
    {
        /// <summary>
        /// Extracts segments for the given tile.
        /// </summary>
        public static Segment[] ExtractSegments(this RouterDb routerDb, ulong tileId,
            SegmentLayerConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }
            if (config.Name == null) { throw new ArgumentException("Layer configuration has no name set."); }

            var tile = new Tile(tileId);
            var diffX = (tile.Top - tile.Bottom);
            var diffY = (tile.Right - tile.Left);
            var marginX = diffX / 1024;
            var marginY = diffY / 1024;

            var tileBox = new LocalGeo.Box(tile.Bottom - marginY, tile.Left - marginX, 
                tile.Top + marginY, tile.Right + marginX);
            var segments = new List<Segment>();

            var vertices = HilbertExtensions.Search(routerDb.Network.GeometricGraph,
                tileBox.MinLat - diffY, tileBox.MinLon - diffX, 
                tileBox.MaxLat + diffY, tileBox.MaxLon + diffX);
            var edges = new HashSet<long>();

            var edgeEnumerator = routerDb.Network.GetEdgeEnumerator();
            foreach (var vertex in vertices)
            {
                var coordinateFrom = routerDb.Network.GetVertex(vertex);

                edgeEnumerator.MoveTo(vertex);
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
                    if (config != null && config.IncludeProfileFunc != null &&
                        !config.IncludeProfileFunc(edgeData.Profile, edgeData.MetaId))
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

                            segments.Add(new Segment()
                            {
                                Meta = edgeData.MetaId,
                                Profile = edgeData.Profile,
                                Shape = shape.ToArray()
                            });
                            shape.Clear();
                            previous = false;
                        }
                    }

                    if (shape.Count >= 2)
                    {
                        segments.Add(new Segment()
                        {
                            Meta = edgeData.MetaId,
                            Profile = edgeData.Profile,
                            Shape = shape.ToArray()
                        });
                        shape.Clear();
                    }
                }
            }

            return segments.ToArray();
        }

        /// <summary>
        /// Extracts a segment layer for the given tile.
        /// </summary>
        public static SegmentLayer ExtractSegmentLayer(this RouterDb routerDb, ulong tileId,
            SegmentLayerConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }
            if (config.Name == null) { throw new ArgumentException("Layer configuration has no name set."); }

            return new SegmentLayer()
            {
                Meta = routerDb.EdgeMeta,
                Profiles = routerDb.EdgeProfiles,
                Name = config.Name,
                Segments = routerDb.ExtractSegments(tileId, config)
            };
        }

        /// <summary>
        /// Extracts a vector tile of the given tile.
        /// </summary>
        public static VectorTile ExtractTile(this RouterDb routerDb, ulong tileId, 
            SegmentLayerConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }
            if (config.Name == null) { throw new ArgumentException("Layer configuration has no name set."); }
            
            var layers = new List<Layer>(1);
            layers.Add(routerDb.ExtractSegmentLayer(tileId, config));

            return new VectorTile()
            {
                Layers = layers,
                TileId = tileId
            };
        }
    }
}