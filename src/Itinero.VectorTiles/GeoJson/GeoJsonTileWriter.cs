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

using Itinero.VectorTiles.Layers;
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