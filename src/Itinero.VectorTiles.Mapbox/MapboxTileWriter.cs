using System;
using System.Collections.Generic;
using System.IO;
using Itinero.Attributes;
using Itinero.VectorTiles.Layers;

namespace Itinero.VectorTiles.Mapbox
{
    // see: https://github.com/mapbox/vector-tile-spec/tree/master/2.1
    public static class MapboxTileWriter
    {
        /// <summary>
        /// Writes the tile to the given stream.
        /// </summary>
        public static void Write(this VectorTile vectorTile, Stream stream, uint extent = 4096)
        {
            var tile = new Tiles.Tile(vectorTile.TileId);

            double latitudeStep = (tile.Top - tile.Bottom) / extent;
            double longitudeStep = (tile.Right - tile.Left) / extent;
            double top = tile.Top;
            double left = tile.Left;

            var mapboxTile = new Mapbox.Tile();
            foreach (var localLayer in vectorTile.Layers)
            {
                var layer = new Mapbox.Tile.Layer {Version = 2, Name = localLayer.Name, Extent = extent};

                var keys = new Dictionary<string, uint>();
                var values = new Dictionary<string, uint>();

                if (localLayer is EdgeLayer segmentLayer)
                {
                    var edges = segmentLayer.Edges;

                    for (var i = 0; i < edges.Count; i++)
                    {
                        var feature = new Mapbox.Tile.Feature();

                        var shape = edges[i].Shape;
                        var posX = (int) ((shape[0].Longitude - left) / longitudeStep);
                        var posY = (int) ((top - shape[0].Latitude) / latitudeStep);
                        GenerateMoveTo(feature.Geometry, posX, posY);

                        // generate line to.
                        feature.Geometry.Add(GenerateCommandInteger(2, shape.Length - 1));
                        for (var j = 1; j < shape.Length; j++)
                        {
                            var localPosX = (int) ((shape[j].Longitude - left) / longitudeStep);
                            var localPosY = (int) ((top - shape[j].Latitude) / latitudeStep);
                            var dx = localPosX - posX;
                            var dy = localPosY - posY;
                            posX = localPosX;
                            posY = localPosY;

                            feature.Geometry.Add(GenerateParameterInteger(dx));
                            feature.Geometry.Add(GenerateParameterInteger(dy));
                        }

                        feature.Type = Tile.GeomType.LineString;

                        var attributes = segmentLayer.Config.GetAttributesFunc(edges[i].EdgeId, tile.Zoom);
                        AddAttributes(feature.Tags, keys, values, attributes);

                        layer.Features.Add(feature);
                    }
                }
                else if (localLayer is VertexLayer vertexLayer)
                {
                    var vertices = vertexLayer.Vertices;

                    for (var i = 0; i < vertices.Count; i++)
                    {
                        var vertex = vertices[i];
                        var vertexMeta = vertexLayer.Config.GetAttributesFunc(vertex.Id, tile.Zoom);
                        
                        var feature = new Mapbox.Tile.Feature();

                        var posX = (int) ((vertex.Longitude - left) / longitudeStep);
                        var posY = (int) ((top - vertex.Latitude) / latitudeStep);
                        GenerateMoveTo(feature.Geometry, posX, posY);
                        feature.Type = Tile.GeomType.Point;
                        
                        AddAttributes(feature.Tags, keys, values, vertexMeta);

                        layer.Features.Add(feature);
                    }
                }
                else
                { // unknown type of layer.
                    continue;
                }

                layer.Keys.AddRange(keys.Keys);
                foreach (var value in values.Keys)
                {
                    if (int.TryParse(value, out var intValue))
                    {
                        layer.Values.Add(new Tile.Value()
                        {
                            IntValue = intValue
                        });
                    }
                    else if (float.TryParse(value, out var floatValue))
                    {
                        layer.Values.Add(new Tile.Value()
                        {
                            FloatValue = floatValue
                        });
                    }
                    else
                    {
                        layer.Values.Add(new Tile.Value()
                        {
                            StringValue = value
                        });
                    }
                }
                mapboxTile.Layers.Add(layer);
            }

            ProtoBuf.Serializer.Serialize<Tile>(stream, mapboxTile);
        }

        private static void AddAttributes(List<uint> tags, Dictionary<string, uint> keys,
            Dictionary<string, uint> values, IEnumerable<Attributes.Attribute> attributes)
        {
            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    tags.Add(keys.AddOrGet(attribute.Key));
                    tags.Add(values.AddOrGet(attribute.Value));
                }
            }
        }

        /// <summary>
        /// Generates a move command. 
        /// </summary>
        private static void GenerateMoveTo(List<uint> geometry, int dx, int dy)
        {
            geometry.Add(GenerateCommandInteger(1, 1));
            geometry.Add(GenerateParameterInteger(dx));
            geometry.Add(GenerateParameterInteger(dy));
        }

        /// <summary>
        /// Generates a close path command.
        /// </summary>
        private static void GenerateClosePath(List<uint> geometry)
        {
            geometry.Add(GenerateCommandInteger(7, 1));
        }

        /// <summary>
        /// Generates a command integer.
        /// </summary>
        private static uint GenerateCommandInteger(int id, int count)
        { // CommandInteger = (id & 0x7) | (count << 3)
            return (uint) ((id & 0x7) | (count << 3));
        }

        /// <summary>
        /// Generates a parameter integer.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static uint GenerateParameterInteger(int value)
        { // ParameterInteger = (value << 1) ^ (value >> 31)
            return (uint) ((value<<1) ^ (value>> 31));
        }
    }
}