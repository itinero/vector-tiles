

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
            var tileRange = tile.GetSubTiles(11);

            foreach (var t in tileRange)
            {
                var segments = routerDb.ExtractTile(t.Id);

                //var tileGeoJson = Itinero.VectorTiles.GeoJson.GeoJsonTileWriter.ToGeoJson(segments, routerDb);

                using (var stream = File.Open(t.Id.ToInvariantString() + ".mvt", FileMode.Create))
                {
                    Itinero.VectorTiles.Mapbox.MapboxTileWriter.Write(segments, t, routerDb, 4096, stream);

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
