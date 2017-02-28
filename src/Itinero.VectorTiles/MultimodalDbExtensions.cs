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
using Itinero.VectorTiles.Layers;
using System.Collections.Generic;

namespace Itinero.VectorTiles
{
    /// <summary>
    /// Contains extension methods for the multimodal db.
    /// </summary>
    public static class MultimodalDbExtensions
    {
        /// <summary>
        /// Extract data for the given tile.
        /// </summary>
        public static VectorTile ExtractTile(this MultimodalDb db, uint tileId, string segmentsName = "transportation",
            string stopsName = "stops")
        {
            var layers = new List<Layer>();

            if (string.IsNullOrWhiteSpace(segmentsName))
            {
                layers.Add(db.RouterDb.ExtractSegmentLayer(tileId, segmentsName));
            }
            if (string.IsNullOrWhiteSpace(stopsName))
            {
                layers.Add(db.TransitDb.ExtractPointLayerForStops(tileId, stopsName));
            }

            return new VectorTile()
            {
                Layers = layers,
                TileId = tileId
            };
        }
    }
}