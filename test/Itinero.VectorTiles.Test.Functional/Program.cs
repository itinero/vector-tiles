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

using Itinero.VectorTiles.GeoJson;
using Mapbox.Vector.Tile;
using System.Collections.Generic;
using System.IO;

namespace Itinero.VectorTiles.Test.Functional
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var routerDb = RouterDb.Deserialize(File.OpenRead(@"C:\work\data\routing\belgium.c.cf.routerdb"));

            var tile = Tiles.Tile.CreateAroundLocation(51.267966846313556f, 4.801913201808929f, 10);
            var tileRange = tile.GetSubTiles(14);

            foreach (var t in tileRange)
            {
                var vectorTile = routerDb.ExtractTile(t.Id, "transportation");

                //var tileGeoJson = Itinero.VectorTiles.GeoJson.GeoJsonTileWriter.ToGeoJson(segments, routerDb);

                using (var stream = File.Open(t.Id.ToInvariantString() + ".mvt", FileMode.Create))
                {
                    Itinero.VectorTiles.Mapbox.MapboxTileWriter.Write(vectorTile, stream);

                    //stream.Flush();
                    //stream.Seek(0, SeekOrigin.Begin);
                    //var parsed = global::Mapbox.Vector.Tile.VectorTileParser.Parse(stream);
                    //var geoJson = parsed[0].ToGeoJSON(t.X, t.Y, t.Zoom);
                    //var json = Newtonsoft.Json.JsonConvert.SerializeObject(geoJson);

                }
            }
        }

        //private static string ToJson(FeatureCollection featureCollection)
        //{
        //    var jsonSerializer = new NetTopologySuite.IO.GeoJsonSerializer();
        //    var jsonStream = new StringWriter();
        //    jsonSerializer.Serialize(jsonStream, featureCollection);
        //    var json = jsonStream.ToInvariantString();
        //    return json;
        //}
    }
}
