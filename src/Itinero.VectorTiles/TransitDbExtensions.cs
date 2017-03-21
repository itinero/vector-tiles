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

using Itinero.Transit.Data;
using Itinero.Transit.Algorithms.Search;
using Itinero.VectorTiles.Tiles;
using Itinero.VectorTiles.Layers;
using System;
using System.Collections.Generic;

namespace Itinero.VectorTiles
{
    /// <summary>
    /// Contains transit db extensions.
    /// </summary>
    public static class TransitDbExtensions
    {
        /// <summary>
        /// Extracts a tile of stops.
        /// </summary>
        public static Stop[] ExtractPointsForStops(this TransitDb transitDb, ulong tileId,
            StopLayerConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }
            if (config.Name == null) { throw new ArgumentException("Layer configuration has no name set."); }

            if (transitDb.StopsCount == 0)
            {
                return new Stop[0];
            }

            var tile = new Tile(tileId);
            var diffX = (tile.Top - tile.Bottom);
            var diffY = (tile.Right - tile.Left);
            var marginX = diffX / 1024;
            var marginY = diffY / 1024;

            var tileBox = new LocalGeo.Box(tile.Bottom - marginY, tile.Left - marginX,
                tile.Top + marginY, tile.Right + marginX);

            var stopsEnumerator = transitDb.GetStopsEnumerator();
            var stopIds = stopsEnumerator.Search(
                tileBox.MinLat - diffY, tileBox.MinLon - diffX,
                tileBox.MaxLat + diffY, tileBox.MaxLon + diffX);

            var stops = new Stop[stopIds.Count];
            var i = 0;
            foreach (var stopId in stopIds)
            {
                stopsEnumerator.MoveTo(stopId);

                if (config != null && config.IncludeStopsFunc != null &&
                   !config.IncludeStopsFunc(stopsEnumerator))
                { // explicitly excluded this stop.
                    continue;
                }

                stops[i] = new Stop()
                {
                    Latitude = stopsEnumerator.Latitude,
                    Longitude = stopsEnumerator.Longitude,
                    MetaId = stopsEnumerator.MetaId
                };

                i++;
            }
            return stops;
        }
        
        /// <summary>
        /// Extracts a tile of stops.
        /// </summary>
        public static StopLayer ExtractPointLayerForStops(this TransitDb transitDb, ulong tileId,
            StopLayerConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }
            if (config.Name == null) { throw new ArgumentException("Layer configuration has no name set."); }

            return new StopLayer()
            {
                Meta = transitDb.StopAttributes,
                Name = config.Name,
                Points = transitDb.ExtractPointsForStops(tileId, config)
            };
        }

        /// <summary>
        /// Extracts a tile of stops.
        /// </summary>
        public static VectorTile ExtractTileForStops(this TransitDb transitDb, ulong tileId,
            StopLayerConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }
            if (config.Name == null) { throw new ArgumentException("Layer configuration has no name set."); }

            var layers = new List<Layer>(1);
            layers.Add(transitDb.ExtractPointLayerForStops(tileId, config));

            return new VectorTile()
            {
                Layers = layers,
                TileId = tileId
            };
        }
    }
}