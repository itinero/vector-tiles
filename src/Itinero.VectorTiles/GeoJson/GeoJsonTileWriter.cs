using System;
using System.IO;

namespace Itinero.VectorTiles.GeoJson
{
    /// <summary>
    /// A geosjon tile writer.
    /// </summary>
    public static class GeoJsonTileWriter
    {
        /// <summary>
        /// Converts the given segments to geojson.
        /// </summary>
        public static string ToGeoJson(this Segment[] segments, RouterDb routerDb)
        {
            var stringWriter = new StringWriter();
            segments.WriteGeoJson(routerDb, stringWriter);
            return stringWriter.ToInvariantString();
        }

        /// <summary>
        /// Writes the tile as geojson.
        /// </summary>
        public static void WriteGeoJson(this Segment[] segments, RouterDb routerDb, TextWriter writer)
        {
            if (segments == null) { throw new ArgumentNullException("segments"); }
            if (writer == null) { throw new ArgumentNullException("writer"); }

            var edgeProfile = routerDb.EdgeProfiles;
            var edgeMeta = routerDb.EdgeMeta;

            var jsonWriter = new IO.Json.JsonWriter(writer);
            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "FeatureCollection", true, false);
            jsonWriter.WritePropertyName("features", false);
            jsonWriter.WriteArrayOpen();

            for (var i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                var coordinates = segment.Shape;

                if (coordinates.Length >= 2)
                {
                    jsonWriter.WriteOpen();
                    jsonWriter.WriteProperty("type", "Feature", true, false);
                    jsonWriter.WritePropertyName("geometry", false);

                    jsonWriter.WriteOpen();
                    jsonWriter.WriteProperty("type", "LineString", true, false);
                    jsonWriter.WritePropertyName("coordinates", false);
                    jsonWriter.WriteArrayOpen();
                    for (var shape = 0; shape < coordinates.Length; shape++)
                    {
                        jsonWriter.WriteArrayOpen();
                        jsonWriter.WriteArrayValue(coordinates[shape].Longitude.ToInvariantString());
                        jsonWriter.WriteArrayValue(coordinates[shape].Latitude.ToInvariantString());
                        jsonWriter.WriteArrayClose();
                    }
                    jsonWriter.WriteArrayClose();
                    jsonWriter.WriteClose();

                    jsonWriter.WritePropertyName("properties");
                    jsonWriter.WriteOpen();
                    var profile = edgeProfile.Get(segment.Profile);
                    if (profile != null)
                    {
                        foreach (var attribute in profile)
                        {
                            jsonWriter.WriteProperty(attribute.Key, attribute.Value, true, true);
                        }
                    }
                    var meta = edgeMeta.Get(segment.Meta);
                    if (meta != null)
                    {
                        foreach (var attribute in meta)
                        {
                            jsonWriter.WriteProperty(attribute.Key, attribute.Value, true, true);
                        }
                    }
                    jsonWriter.WriteClose();

                    jsonWriter.WriteClose();
                }
            }

            jsonWriter.WriteArrayClose();
            jsonWriter.WriteClose();
        }
    }
}