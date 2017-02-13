using Itinero.VectorTiles.Tiles;
using System.Collections.Generic;
using Itinero.Algorithms.Search.Hilbert;
using Itinero.LocalGeo;
using Itinero.Graphs.Geometric;
using Itinero.Data.Network;

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
        public static Segment[] ExtractTile(this RouterDb routerDb, ulong tileId)
        {
            var tile = new Tile(tileId);
            var tileBox = new LocalGeo.Box(tile.Bottom, tile.Left, tile.Top, tile.Right);
            var segments = new List<Segment>();

            var vertices = HilbertExtensions.Search(routerDb.Network.GeometricGraph,
                tileBox.MinLat, tileBox.MinLon, tileBox.MaxLat, tileBox.MaxLon);
            var edges = new HashSet<long>();
            //var enumShape = new Coordinate[1024];

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

                    // get shape.
                    var coordinateTo = routerDb.Network.GetVertex(edgeEnumerator.To);
                    //var enumShapeCount = edgeEnumerator.FillShape(coordinateFrom, coordinateTo, enumShape);
                    var shape = new List<Coordinate>();
                    var enumShape = routerDb.Network.GetShape(edgeEnumerator.Current);
                    
                    // loop over shape.
                    var edgeData = edgeEnumerator.Data;
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

        ///// <summary>
        ///// Fills the given array as efficiently as possible with shape points.
        ///// </summary>
        //private static int FillShape(this Data.Network.RoutingNetwork.EdgeEnumerator enumerator, Coordinate coordinateFrom, Coordinate coordinateTo, Coordinate[] shape)
        //{
        //    var size = 0;
        //    shape[0] = coordinateFrom;
        //    size++;

        //    var enumShape = enumerator.Shape;
        //    if (enumShape != null)
        //    {
        //        if (enumerator.DataInverted)
        //        {
        //            enumShape = enumShape.Reverse();
        //        }
        //        for(var i = 0; i < enumShape.Count; i++)
        //        {
        //            shape[size] = enumShape[i];
        //            size++;
        //        }
        //    }

        //    shape[size] = coordinateTo;
        //    size++;

        //    return size;
        //}
    }
}