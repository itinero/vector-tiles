using Itinero.VectorTiles.Tiles;
using System.Collections.Generic;
using Itinero.Algorithms.Search.Hilbert;
using Itinero.LocalGeo;
using Itinero.Graphs.Geometric;
using Itinero.Data.Network;
using System;

namespace Itinero.VectorTiles
{
    /// <summary>
    /// Contains routerdb extensions.
    /// </summary>
    public static class RouterDbExtensions
    {
        /// <summary>
        /// Extracts one tile for the given tile.
        /// </summary>
        public static Segment[] ExtractTile(this RouterDb routerDb, ulong tileId, 
            Func<ushort, uint, bool> includeProfile)
        {
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
                    if (includeProfile != null &&
                        !includeProfile(edgeData.Profile, edgeData.MetaId))
                    { // include profile returns false
                        continue;
                    }

                    // get shape.
                    var coordinateTo = routerDb.Network.GetVertex(edgeEnumerator.To);
                    //var enumShapeCount = edgeEnumerator.FillShape(coordinateFrom, coordinateTo, enumShape);
                    var shape = new List<Coordinate>();
                    var enumShape = routerDb.Network.GetShape(edgeEnumerator.Current);

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
    }
}