using Itinero.Attributes;
using System;
using System.Collections.Generic;
using System.IO;

namespace Itinero.VectorTiles.Mapbox
{
    // see: https://github.com/mapbox/vector-tile-spec/tree/master/2.1
    public static class MapboxTileWriter
    {
        /// <summary>
        /// Writes the tile to the given stream.
        /// </summary>
        public static void Write(this Segment[] segments, Itinero.VectorTiles.Tiles.Tile tile, string layerName, RouterDb routerDb, uint extent, Stream stream,
            Func<IAttributeCollection, IAttributeCollection> mapAttributes)
        {
            var keys = new Dictionary<string, uint>();
            var values = new Dictionary<string, uint>();
            var edgeProfile = routerDb.EdgeProfiles;
            var edgeMeta = routerDb.EdgeMeta;

            double latitudeStep = (tile.Top - tile.Bottom) / extent;
            double longitudeStep = (tile.Right - tile.Left) / extent;
            double top = tile.Top;
            double left = tile.Left;
            
            var layer = new Mapbox.Tile.Layer();
            layer.Version = 2;
            layer.Name = layerName;
            layer.Extent = extent;
            for (var i = 0; i < segments.Length; i++)
            {
                var feature = new Mapbox.Tile.Feature();

                var shape = segments[i].Shape;
                var posX = (int)((shape[0].Longitude - left) / longitudeStep);
                var posY = (int)((top - shape[0].Latitude) / latitudeStep);
                GenerateMoveTo(feature.Geometry, posX, posY);

                // generate line to.
                feature.Geometry.Add(GenerateCommandInteger(2, shape.Length - 1));
                for (var j = 1; j < shape.Length; j++)
                {
                    var localPosX = (int)((shape[j].Longitude - left) / longitudeStep);
                    var localPosY = (int)((top - shape[j].Latitude) / latitudeStep);
                    var dx = localPosX - posX;
                    var dy = localPosY - posY;
                    posX = localPosX;
                    posY = localPosY;

                    feature.Geometry.Add(GenerateParameterInteger(dx));
                    feature.Geometry.Add(GenerateParameterInteger(dy));
                }

                feature.Type = Tile.GeomType.LineString;

                if (mapAttributes != null)
                {
                    IAttributeCollection attributes = new AttributeCollection(edgeProfile.Get(segments[i].Profile));
                    var meta = edgeMeta.Get(segments[i].Meta);
                    foreach(var a in meta)
                    {
                        attributes.AddOrReplace(a);
                    }

                    attributes = mapAttributes(attributes);

                    foreach(var attribute in attributes)
                    {
                        uint keyId;
                        if (!keys.TryGetValue(attribute.Key, out keyId))
                        {
                            keyId = (uint)keys.Count;
                            keys.Add(attribute.Key, keyId);
                        }
                        uint valueId;
                        if (!values.TryGetValue(attribute.Value, out valueId))
                        {
                            valueId = (uint)values.Count;
                            values.Add(attribute.Value, valueId);
                        }
                        feature.Tags.Add(keyId);
                        feature.Tags.Add(valueId);
                    }
                }
                else
                {
                    var profile = edgeProfile.Get(segments[i].Profile);
                    if (profile != null)
                    {
                        foreach (var attribute in profile)
                        {
                            uint keyId;
                            if (!keys.TryGetValue(attribute.Key, out keyId))
                            {
                                keyId = (uint)keys.Count;
                                keys.Add(attribute.Key, keyId);
                            }
                            uint valueId;
                            if (!values.TryGetValue(attribute.Value, out valueId))
                            {
                                valueId = (uint)values.Count;
                                values.Add(attribute.Value, valueId);
                            }
                            feature.Tags.Add(keyId);
                            feature.Tags.Add(valueId);
                        }
                    }
                    var meta = edgeMeta.Get(segments[i].Meta);
                    if (meta != null)
                    {
                        foreach (var attribute in meta)
                        {
                            uint keyId;
                            if (!keys.TryGetValue(attribute.Key, out keyId))
                            {
                                keyId = (uint)keys.Count;
                                keys.Add(attribute.Key, keyId);
                            }
                            uint valueId;
                            if (!values.TryGetValue(attribute.Value, out valueId))
                            {
                                valueId = (uint)values.Count;
                                values.Add(attribute.Value, valueId);
                            }
                            feature.Tags.Add(keyId);
                            feature.Tags.Add(valueId);
                        }
                    }
                }

                layer.Features.Add(feature);
            }
            layer.Keys.AddRange(keys.Keys);
            foreach(var value in values.Keys)
            {
                layer.Values.Add(new Tile.Value()
                {
                    StringValue = value
                });
            }

            var mapboxTile = new Mapbox.Tile();
            mapboxTile.Layers.Add(layer);

            ProtoBuf.Serializer.Serialize<Tile>(stream, mapboxTile);
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
            return (uint)((id & 0x7) | (count << 3));
        }

        /// <summary>
        /// Generates a parameter integer.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static uint GenerateParameterInteger(int value)
        { // ParameterInteger = (value << 1) ^ (value >> 31)
            return (uint)((value << 1) ^ (value >> 31));
        }
    }
}