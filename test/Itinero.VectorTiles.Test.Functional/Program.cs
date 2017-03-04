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
using Itinero.VectorTiles.GeoJson;
using Itinero.VectorTiles.Layers;
using Mapbox.Vector.Tile;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;

namespace Itinero.VectorTiles.Test.Functional
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Itinero.Logging.Logger.LogAction = (o, level, message, parameters) =>
            {
                Log.Information(string.Format("[{0}] {1} - {2}", o, level, message));
            };

            // attach logger.
            Log.Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .CreateLogger();
       
            // load multimodal db and extract tiles.
            var multimodalDb = MultimodalDb.Deserialize(File.OpenRead(@"C:\work\data\routing\belgium.multimodaldb"));
            var tile = Tiles.Tile.CreateAroundLocation(51.267966846313556f, 4.801913201808929f, 9);
            var tileRange = tile.GetSubTiles(14);

            var func = new Action(() =>
            {
                foreach (var t in tileRange)
                {
                    var config = new VectorTileConfig()
                    {
                        SegmentLayerConfig = new SegmentLayerConfig()
                        {
                            Name = "transportation"
                        },
                        StopLayerConfig = new StopLayerConfig()
                        {
                            Name = "stops"
                        }
                    };

                    //Log.Information("Extracting tile: {0}", t.ToInvariantString());
                    var vectorTile = multimodalDb.ExtractTile(t.Id, config);

                    //Log.Information("Writing tile: {0}", t.ToInvariantString());
                    using (var stream = File.Open(t.Id.ToInvariantString() + ".mvt", FileMode.Create))
                    {
                        Itinero.VectorTiles.Mapbox.MapboxTileWriter.Write(vectorTile, stream);
                    }
                }
            });
            func.TestPerf(string.Format("Extracted and written {0} tiles.", tileRange.Count));

            Console.ReadLine();
        }
    }
}